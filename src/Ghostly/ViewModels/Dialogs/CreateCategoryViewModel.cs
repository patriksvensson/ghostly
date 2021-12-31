using Ghostly.Core.Mvvm;
using Ghostly.Core.Services;
using Ghostly.Core.Text;
using Ghostly.Domain;
using Ghostly.Features.Querying;

namespace Ghostly.ViewModels.Dialogs
{
    public sealed class CreateCategoryViewModel
        : DialogScreen<CreateCategoryViewModel.Request, CreateCategoryViewModel.Response>, IValidatableDialog
    {
        private readonly ILocalizer _localizer;

        public bool IsValid => IsFilter ? (HasName && HasExpression && FilterAllowed) : HasName && CategoryAllowed;
        public bool HasName => !string.IsNullOrWhiteSpace(Name);
        public bool HasExpression => !string.IsNullOrWhiteSpace(Expression);
        public bool HasExpressionError => !string.IsNullOrWhiteSpace(ExpressionError);
        public bool IsEditing { get; set; }
        public bool CategoryAllowed { get; set; }
        public bool FilterAllowed { get; set; }

        public string DefaultGlyph
        {
            get
            {
                return IsFilter
                    ? Constants.Glyphs.Filter
                    : Constants.Glyphs.Category;
            }
        }

        public string Emoji
        {
            get; set;
        }

        public string Title => _localizer.GetString(IsEditing,
            IsFilter ? "CreateCategory_Title_EditFilter" : "CreateCategory_Title_EditCategory",
            IsFilter ? "CreateCategory_Title_NewFilter" : "CreateCategory_Title_NewCategory");

        public string ButtonText => _localizer.GetString(IsEditing, "General_Save", "General_Create");
        public string ImportedInfo { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                NotifyPropertyChanged(nameof(IsValid));
            }
        }

        private bool _isFilter;
        public bool IsFilter
        {
            get => _isFilter;
            set
            {
                SetProperty(ref _isFilter, value);
                NotifyPropertyChanged(nameof(Title));
                NotifyPropertyChanged(nameof(IsValid));
                NotifyPropertyChanged(nameof(DefaultGlyph));
            }
        }

        private bool _showTotal;
        public bool ShowTotal
        {
            get => _showTotal;
            set
            {
                SetProperty(ref _showTotal, value);
            }
        }

        private bool _muted;
        public bool Muted
        {
            get => _muted;
            set
            {
                SetProperty(ref _muted, value);
            }
        }

        private bool _includeMuted;
        public bool IncludeMuted
        {
            get => _includeMuted;
            set
            {
                SetProperty(ref _includeMuted, value);
            }
        }

        private string _expression;
        public string Expression
        {
            get => _expression;
            set
            {
                if (SetProperty(ref _expression, value))
                {
                    NotifyPropertyChanged(nameof(IsValid));
                    ExpressionError = string.Empty;
                }
            }
        }

        private string _expressionError;
        public string ExpressionError
        {
            get => _expressionError;
            set
            {
                if (SetProperty(ref _expressionError, value))
                {
                    NotifyPropertyChanged(nameof(HasExpressionError));
                }
            }
        }

        public sealed class Request : IDialogRequest<Response>
        {
            public Category Category { get; set; }
        }

        public sealed class Response
        {
            public string Name { get; set; }
            public string Expression { get; set; }
            public string Emoji { get; set; }
            public bool ShowTotal { get; set; }
            public bool Muted { get; set; }
            public bool IncludeMuted { get; set; }
        }

        public CreateCategoryViewModel(ILocalizer localizer)
        {
            _localizer = localizer ?? throw new System.ArgumentNullException(nameof(localizer));
        }

        public override void Initialize(Request request)
        {
            if (request is null)
            {
                throw new System.ArgumentNullException(nameof(request));
            }

            IsEditing = request.Category != null;

            if (request.Category != null)
            {
                Name = request.Category.Name;
                Expression = request.Category.Expression;
                Emoji = request.Category.Emoji;
                ShowTotal = request.Category.ShowTotal;
                Muted = request.Category.Muted;
                IncludeMuted = request.Category.IncludeMuted;
                IsFilter = !string.IsNullOrWhiteSpace(Expression);

                CategoryAllowed = !IsFilter;
                FilterAllowed = IsFilter;

                if (!string.IsNullOrWhiteSpace(request.Category.ImportedFrom))
                {
                    ImportedInfo = _localizer.Format("CreateCategory_CategoryImported", request.Category.ImportedFrom);
                }
            }
            else
            {
                CategoryAllowed = true;
                FilterAllowed = true;
            }

            NotifyPropertyChanged(nameof(Title));
            NotifyPropertyChanged(nameof(ButtonText));
        }

        public bool Validate()
        {
            if (!IsValid)
            {
                return false;
            }

            if (IsFilter)
            {
                if (!GhostlyQueryLanguage.TryCompile(Expression, out var _, out var error))
                {
                    ExpressionError = error;
                    return false;
                }
            }

            return true;
        }

        public override Response GetResult(bool ok)
        {
            if (ok)
            {
                return new Response
                {
                    Name = _name,
                    Expression = IsFilter ? _expression : null,
                    Emoji = EmojiHelper.GetEmojiShortcode(Emoji),
                    ShowTotal = ShowTotal,
                    Muted = Muted,
                    IncludeMuted = IncludeMuted,
                };
            }

            return null;
        }
    }
}
