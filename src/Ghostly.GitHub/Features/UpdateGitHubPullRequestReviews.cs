using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.GitHub.Octokit;
using MediatR;
using Octokit;

namespace Ghostly.GitHub.Actions
{
    internal sealed class UpdateGitHubPullRequestReviews : GitHubRequestHandler<UpdateGitHubPullRequestReviews.Request>
    {
        private readonly IGhostlyLog _log;
        private readonly IMediator _mediator;
        private readonly ITelemetry _telemetry;

        public sealed class Request : IRequest<GitHubResult>
        {
            public IGitHubGateway Gateway { get; }
            public GhostlyContext Context { get; }
            public WorkItemData Workitem { get; }

            public Request(IGitHubGateway gateway, GhostlyContext context, WorkItemData workitem)
            {
                Gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
                Context = context ?? throw new ArgumentNullException(nameof(context));
                Workitem = workitem ?? throw new ArgumentNullException(nameof(workitem));
            }
        }

        public UpdateGitHubPullRequestReviews(
            IMediator mediator,
            ITelemetry telemetry,
            IGhostlyLog log)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        protected override async Task<GitHubResult> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            if (!request.Workitem.IsPullRequest.GetSafeValue())
            {
                return GitHubResult.Ok();
            }

            // TODO: Move validation
            if (request.Workitem.GitHubLocalId.GetSafeValue(0) == 0)
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize review since it's pull request is missing a number.");
                return GitHubResult.Ok();
            }
            else if (request.Workitem.Repository == null)
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize review since its missing repository.");
                return GitHubResult.Ok();
            }
            else if (request.Workitem.Repository.Owner == null)
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize review since its missing repository owner.");
                return GitHubResult.Ok();
            }
            else if (string.IsNullOrWhiteSpace(request.Workitem.Repository.Owner))
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize review since the repository login is empty.");
                return GitHubResult.Ok();
            }
            else if (request.Workitem.Repository.Name == null)
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize review since its missing repository name.");
                return GitHubResult.Ok();
            }
            else if (string.IsNullOrWhiteSpace(request.Workitem.Repository.Name))
            {
                _telemetry.TrackAndLogError(_log, "Can't synchronize review since the repository name is empty.");
                return GitHubResult.Ok();
            }

            _log.Debug("Updating reviews for pull request {PullRequestId}...", request.Workitem.GitHubId);

            // Get all review comments.
            var githubCommentsResult = await request.Gateway.GetReviewComments(
                request.Workitem.Repository.Owner,
                request.Workitem.Repository.Name,
                request.Workitem.GitHubLocalId.Value);

            if (githubCommentsResult.Faulted)
            {
                return githubCommentsResult.ForCaller(nameof(UpdateGitHubPullRequestReviews))
                    .Track(_telemetry, "Could not get comments for review.")
                    .Log(_log, "Could not get comments for review {GitHubReviewId}.", request.Workitem.GitHubId)
                    .GetResult();
            }

            // Get all reviews.
            var reviewsResult = await request.Gateway.GetReviews(
                request.Workitem.Repository.Owner,
                request.Workitem.Repository.Name,
                request.Workitem.GitHubLocalId.Value);

            if (reviewsResult.Faulted)
            {
                return reviewsResult.ForCaller(nameof(UpdateGitHubPullRequestReviews))
                    .Track(_telemetry, "Could not get reviews for pull request.")
                    .Log(_log, "Could not get reviews for pull request {GitHubWorkItemId}.", request.Workitem.Id)
                    .GetResult();
            }

            var githubComments = githubCommentsResult.Unwrap();
            var reviews = reviewsResult.Unwrap();

            foreach (var githubReview in reviews)
            {
                if (string.IsNullOrWhiteSpace(githubReview.User.Login))
                {
                    _telemetry.TrackAndLogError(_log, "Can't synchronize review since it's missing a user.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(githubReview.User.Login))
                {
                    _telemetry.TrackAndLogError(_log, "Can't synchronize review since the user login is empty.");
                    continue;
                }

                var reviewResult = await UpdateReview(request, githubReview);
                if (reviewResult.Faulted)
                {
                    return reviewResult.ForCaller(nameof(UpdateGitHubPullRequestReviews))
                        .Track(_telemetry, "Could not update review.")
                        .Log(_log, "Could not update review.")
                        .GetResult();
                }

                var reviewData = reviewResult.Unwrap();
                if (reviewData != null)
                {
                    var commentsResult = await UpdateComments(request, reviewData, githubComments);
                    if (commentsResult.Faulted)
                    {
                        return commentsResult.ForCaller(nameof(UpdateGitHubPullRequestReviews))
                            .Track(_telemetry, "Could not update review comments.")
                            .Log(_log, "Could not update comments for review {GitHubReviewId}.", githubReview.Id)
                            .GetResult();
                    }
                }
            }

            return GitHubResult.Ok();
        }

        private async Task<GitHubResult<ReviewData>> UpdateReview(Request request, PullRequestReview githubReview)
        {
            var reviewData = request.Workitem.Reviews?.FirstOrDefault(r => r.GitHubId == githubReview.Id);
            if (reviewData == null)
            {
                var authorResult = await _mediator.Send(new GetGitHubUser.Request(request.Gateway, request.Context, githubReview.User.Login, true));
                if (authorResult.Faulted)
                {
                    return authorResult.ForCaller(nameof(UpdateGitHubPullRequestReviews))
                        .Track(_telemetry, "Could not retrieve user who created the review.")
                        .Log(_log, "Could not retrieve user who created the review.")
                        .GetResult().Convert<ReviewData>();
                }

                // Create the review.
                reviewData = new ReviewData
                {
                    Discriminator = Discriminator.GitHub,
                    GitHubId = githubReview.Id,
                    CreatedAt = githubReview.SubmittedAt.DateTime.EnsureUniversalTime(),
                    Author = authorResult.Unwrap(),
                    State = GetReviewState(githubReview.State.Value),
                    Url = githubReview.HtmlUrl,
                    Body = githubReview.Body,
                };

                // Add the review to the pull request.
                request.Workitem.Reviews = request.Workitem.Reviews ?? new List<ReviewData>();
                request.Workitem.Reviews.Add(reviewData);
            }
            else
            {
                // Update the review.
                reviewData.State = GetReviewState(githubReview.State.Value);
                reviewData.Body = githubReview.Body;
                request.Context.Reviews.Update(reviewData);
            }

            return GitHubResult.Ok(reviewData);
        }

        private async Task<GitHubResult> UpdateComments(Request request, ReviewData review, IReadOnlyList<PullRequestReviewComment> comments)
        {
            foreach (var githubComment in comments.Where(c => c.PullRequestReviewId == review.GitHubId))
            {
                if (githubComment.User == null)
                {
                    _telemetry.TrackAndLogError(_log, "Can't synchronize review comment since it's missing user.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(githubComment.User.Login))
                {
                    _telemetry.TrackAndLogError(_log, "Can't synchronize review comment since the user login is empty.");
                    continue;
                }

                var commentData = review.Comments?.FirstOrDefault(c => c.GitHubId == githubComment.Id);
                if (commentData == null)
                {
                    var authorResult = await _mediator.Send(new GetGitHubUser.Request(request.Gateway, request.Context, githubComment.User.Login, true));
                    if (authorResult.Faulted)
                    {
                        return authorResult.ForCaller(nameof(UpdateGitHubPullRequestReviews))
                            .Track(_telemetry, "Could not retrieve user who created the review comment.")
                            .Log(_log, "Could not retrieve user {GitHubUser} who created the review comment {GitHubReviewCommentId}.",
                                githubComment.User.Login, githubComment.Id)
                            .GetResult();
                    }

                    commentData = new ReviewCommentData
                    {
                        GitHubId = githubComment.Id,
                        CreatedAt = githubComment.CreatedAt.DateTime.EnsureUniversalTime(),
                        UpdatedAt = githubComment.UpdatedAt.DateTime.EnsureUniversalTime(),
                        OriginalCommitId = githubComment.OriginalCommitId,
                        CommitId = githubComment.CommitId,
                        Path = githubComment.Path,
                        Position = githubComment.Position,
                        OriginalPosition = githubComment.OriginalPosition,
                        Diff = githubComment.DiffHunk,
                        Url = githubComment.HtmlUrl,
                        Body = githubComment.Body,
                        InReplyToId = githubComment.InReplyToId,
                        Author = authorResult.Unwrap(),
                    };

                    review.Comments = review.Comments ?? new List<ReviewCommentData>();
                    review.Comments.Add(commentData);
                }
                else
                {
                    // Update comment.
                    commentData.GitHubId = githubComment.Id;
                    commentData.CreatedAt = githubComment.CreatedAt.DateTime.EnsureUniversalTime();
                    commentData.UpdatedAt = githubComment.UpdatedAt.DateTime.EnsureUniversalTime();
                    commentData.OriginalCommitId = githubComment.OriginalCommitId;
                    commentData.CommitId = githubComment.CommitId;
                    commentData.Path = githubComment.Path;
                    commentData.Position = githubComment.Position;
                    commentData.OriginalPosition = githubComment.OriginalPosition;
                    commentData.Diff = githubComment.DiffHunk;
                    commentData.Body = githubComment.Body;

                    request.Context.ReviewComments.Update(commentData);
                }
            }

            return GitHubResult.Ok();
        }

        private ReviewState GetReviewState(PullRequestReviewState state)
        {
            switch (state)
            {
                case PullRequestReviewState.Approved:
                    return ReviewState.Approved;
                case PullRequestReviewState.ChangesRequested:
                    return ReviewState.ChangesRequested;
                case PullRequestReviewState.Commented:
                    return ReviewState.Commented;
                case PullRequestReviewState.Dismissed:
                    return ReviewState.Dismissed;
                case PullRequestReviewState.Pending:
                    return ReviewState.Pending;
                default:
                    return ReviewState.Unknown;
            }
        }
    }
}
