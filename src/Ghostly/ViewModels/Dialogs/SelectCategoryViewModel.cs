using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Services;

namespace Ghostly.ViewModels.Dialogs
{
    public sealed class SelectCategoryViewModel : DialogScreen<SelectCategoryViewModel.Request, Category>
    {
        private readonly ICategoryService _categories;
        private readonly ILocalizer _localizer;
        private Category _selectedCategory;

        public string Title { get; set; }
        public List<Category> Categories { get; private set; }
        public bool HaveSelectedCategory => _selectedCategory != null;
        public string PrimaryButtonTitle { get; set; }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                NotifyPropertyChanged(nameof(HaveSelectedCategory));
            }
        }

        public SelectCategoryViewModel(
            ICategoryService categories,
            ILocalizer localizer)
        {
            _categories = categories;
            _localizer = localizer;
        }

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "By design")]
        public sealed class Request : IDialogRequest<Category>
        {
            public string PrimaryButtonTitle { get; set; }
            public string Title { get; set; }
            public bool IncludeFilters { get; set; }
            public ISet<int> ExcludedCategories { get; set; }
        }

        public override void Initialize(Request request)
        {
            if (request is null)
            {
                throw new System.ArgumentNullException(nameof(request));
            }

            var categories = (IEnumerable<Category>)_categories.Categories;

            if (!request.IncludeFilters)
            {
                categories = categories.Where(x => x.Kind != CategoryKind.Filter);
            }

            if (request.ExcludedCategories?.Count > 0)
            {
                categories = categories.Where(c => !request.ExcludedCategories.Contains(c.Id));
            }

            Title = request.Title ?? _localizer.GetString("SelectCategory_Title");
            Categories = categories.ToList();
            SelectedCategory = Categories.FirstOrDefault();
            PrimaryButtonTitle = request.PrimaryButtonTitle;
        }

        public override Category GetResult(bool ok)
        {
            return ok ? _selectedCategory : null;
        }
    }
}
