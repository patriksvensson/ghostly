using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Mvvm;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Features.Rules;
using Ghostly.Services;
using Ghostly.ViewModels.Commands;
using MediatR;

namespace Ghostly.ViewModels
{
    [DependentOn(typeof(DatabaseInitializer))]
    public sealed class RuleViewModel : Screen, IInitializable
    {
        private readonly IMessageService _messenger;
        private readonly ISynchronizationService _synchronizer;
        private readonly IMediator _mediator;

        public ObservableCollection<Rule> Rules { get; }
        public IAsyncCommand CreateCommand { get; }
        public IAsyncCommand RunAllCommand { get; }
        public IAsyncCommand<Rule> RunCommand { get; }
        public IAsyncCommand<Rule> EditCommand { get; }
        public IAsyncCommand<Rule> DeleteCommand { get; }

        public bool Enabled { get; set; }
        public bool CanRunRules { get; set; }
        public Stateful<bool> CanCreateRules { get; } = new Stateful<bool>();

        public RuleViewModel(
            IDialogService dialog,
            IMessageService messenger,
            ISynchronizationService synchronizer,
            IMediator mediator,
            ILocalizer localizer)
        {
            _messenger = messenger;
            _synchronizer = synchronizer;
            _mediator = mediator;

            Rules = new ObservableCollection<Rule>();
            CreateCommand = new CreateRuleCommand(dialog, mediator, () => Enabled && CanCreateRules.Value);
            RunAllCommand = new RunRulesCommand(dialog, mediator, localizer, () => CanRunRules, () => Enabled);
            EditCommand = new EditRuleCommand(dialog, mediator, () => Enabled);
            DeleteCommand = new DeleteRuleCommand(dialog, mediator, localizer, () => Enabled);
            RunCommand = new RunRuleCommand(dialog, mediator, localizer, () => Enabled);
            Enabled = true;
        }

        public async Task<bool> Initialize(bool background)
        {
            if (!background)
            {
                _messenger.SubscribeOnUIThread<RuleCreated>(OnRuleCreated);
                _messenger.SubscribeOnUIThread<RuleUpdated>(OnRuleUpdated);
                _messenger.SubscribeOnUIThread<RuleDeleted>(OnRuleDeleted);
                _messenger.SubscribeOnUIThread<RuleStateChanged>(OnRuleStateChanged);
                _messenger.SubscribeOnUIThread<SynchronizationAvailabilityChanged>(OnSynchronizationAvailabilityChanged);

                CanRunRules = Enabled && _synchronizer.CanSynchronize;
                CanCreateRules.Value = true;
            }

            if (Rules.Count == 0)
            {
                Rules.AddRange((await _mediator.GetRules()).OrderBy(r => r.SortOrder));
            }

            return !background;
        }

        public async Task UpdateSortOrder()
        {
            var ordering = RuleOrder.Create(Rules);
            await _mediator.Send(new ReorderRulesHandler.Request(ordering));
        }

        private void OnRuleCreated(RuleCreated message)
        {
            Rules.Add(message.Rule);
            CanCreateRules.Value = true;
            NotifyPropertyChanged(nameof(CreateCommand));
        }

        private void OnRuleUpdated(RuleUpdated message)
        {
            var rule = Rules.FirstOrDefault(r => r.Id == message.Rule.Id);
            if (rule != null)
            {
                rule.Name = message.Rule.Name;
                rule.Expression = message.Rule.Expression;
                rule.Description = message.Rule.Description;
            }
        }

        private void OnRuleDeleted(RuleDeleted message)
        {
            var rule = Rules.FirstOrDefault(r => r.Id == message.RuleId);
            if (rule != null)
            {
                Rules.Remove(rule);
                CanCreateRules.Value = true;
                NotifyPropertyChanged(nameof(CreateCommand));
            }
        }

        private void OnRuleStateChanged(RuleStateChanged message)
        {
            Enabled = message.Enabled;

            NotifyPropertyChanged(nameof(Enabled));
            NotifyPropertyChanged(nameof(CreateCommand));
            NotifyPropertyChanged(nameof(EditCommand));
            NotifyPropertyChanged(nameof(DeleteCommand));
            NotifyPropertyChanged(nameof(RunCommand));
            NotifyPropertyChanged(nameof(RunAllCommand));
        }

        private void OnSynchronizationAvailabilityChanged(SynchronizationAvailabilityChanged message)
        {
            CanRunRules = Enabled && _synchronizer.CanSynchronize;
            NotifyPropertyChanged(nameof(RunCommand));
            NotifyPropertyChanged(nameof(RunAllCommand));
        }
    }
}
