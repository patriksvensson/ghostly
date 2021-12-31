using System.Collections.Generic;
using System.Linq;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Domain;
using Ghostly.Features.Querying;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.ViewModels.Dialogs
{
    public sealed class CreateRuleViewModel
        : DialogScreen<CreateRuleViewModel.Request, CreateRuleViewModel.Response>,
          IValidatableDialog
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ILocalizer _localizer;

        public bool IsValid => Validate(compile: false);
        public bool IsEditing { get; set; }
        public string Title => _localizer.GetString(IsEditing, "CreateRule_EditRule", "CreateRule_NewRule");
        public string ButtonText => _localizer.GetString(IsEditing, "General_Save", "General_Create");

        public int Id { get; set; }
        public Stateful<string> Name { get; }
        public Stateful<string> Expression { get; }
        public Stateful<string> ExpressionError { get; }
        public Stateful<bool> Star { get; }
        public Stateful<bool> Mute { get; }
        public Stateful<bool> MarkAsRead { get; }
        public Stateful<bool> Move { get; }
        public Stateful<bool> StopProcessing { get; }
        public Stateful<Category> Category { get; }

        public string ImportedInfo { get; set; }

        public List<Category> Categories { get; }

        public sealed class Request
            : IDialogRequest<Response>
        {
            public int? Id { get; }

            public Request(int? id = null)
            {
                Id = id;
            }
        }

        public sealed class Response
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Expression { get; set; }
            public bool Star { get; set; }
            public bool Mute { get; set; }
            public bool MarkAsRead { get; set; }
            public bool StopProcessing { get; set; }
            public int? CategoryId { get; set; }
        }

        public CreateRuleViewModel(
            IGhostlyContextFactory factory,
            ILocalizer localizer)
        {
            _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            _localizer = localizer ?? throw new System.ArgumentNullException(nameof(localizer));

            Name = new Stateful<string>(v => NotifyPropertyChanged(nameof(IsValid)));
            Star = new Stateful<bool>(v => NotifyPropertyChanged(nameof(IsValid)));
            Mute = new Stateful<bool>(v => NotifyPropertyChanged(nameof(IsValid)));
            MarkAsRead = new Stateful<bool>(v => NotifyPropertyChanged(nameof(IsValid)));
            Move = new Stateful<bool>(v => NotifyPropertyChanged(nameof(IsValid)));
            StopProcessing = new Stateful<bool>(v => NotifyPropertyChanged(nameof(IsValid)));
            Category = new Stateful<Category>(v => NotifyPropertyChanged(nameof(IsValid)));
            Categories = new List<Category>();

            ExpressionError = new Stateful<string>();
            Expression = new Stateful<string>(v =>
            {
                if (!string.IsNullOrWhiteSpace(ExpressionError.Value))
                {
                    ExpressionError.Value = string.Empty;
                }

                NotifyPropertyChanged(nameof(IsValid));
            });
        }

        public override void Initialize(Request request)
        {
            if (request is null)
            {
                throw new System.ArgumentNullException(nameof(request));
            }

            using (var context = _factory.Create())
            {
                // Load all categories.
                var categories = context.Categories.Where(c => c.Kind != CategoryKind.Filter && !c.Inbox);
                Categories.AddRange(CategoryMapper.Map(categories, _localizer));

                if (request.Id != null)
                {
                    IsEditing = true;

                    var rule = RuleMapper.Map(context.Rules.Include(r => r.Category)
                        .SingleOrDefault(r => r.Id == request.Id), _localizer);

                    if (rule != null)
                    {
                        Id = rule.Id;
                        Name.Value = rule.Name;
                        Expression.Value = rule.Expression;
                        Star.Value = rule.Star;
                        Mute.Value = rule.Mute;
                        MarkAsRead.Value = rule.MarkAsRead;
                        StopProcessing.Value = rule.StopProcessing;
                        Category.Value = rule.Category != null ? Categories.SingleOrDefault(x => x.Id == rule.Category.Id) : null;
                        Move.Value = rule.Category != null;

                        if (!string.IsNullOrWhiteSpace(rule.ImportedFrom))
                        {
                            ImportedInfo = _localizer.Format("CreateRule_RuleImported", rule.ImportedFrom);
                        }
                    }
                }

                if (Category.Value == null)
                {
                    Category.Value = Categories.FirstOrDefault();
                }
            }

            NotifyPropertyChanged(nameof(Title));
            NotifyPropertyChanged(nameof(ButtonText));
        }

        public override Response GetResult(bool ok)
        {
            if (ok)
            {
                return new Response
                {
                    Id = Id,
                    Name = Name.Value,
                    Expression = Expression.Value,
                    Star = Star.Value,
                    Mute = Mute.Value,
                    MarkAsRead = MarkAsRead.Value,
                    StopProcessing = StopProcessing.Value,
                    CategoryId = Move.Value ? Category.Value?.Id : null,
                };
            }

            return null;
        }

        public bool Validate()
        {
            return Validate(compile: true);
        }

        private bool Validate(bool compile)
        {
            if (string.IsNullOrWhiteSpace(Name.Value))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(Expression.Value))
            {
                return false;
            }

            if (compile)
            {
                if (!GhostlyQueryLanguage.TryCompile(Expression.Value, out var _, out var error))
                {
                    ExpressionError.Value = error;
                    return false;
                }
            }

            return true;
        }
    }
}
