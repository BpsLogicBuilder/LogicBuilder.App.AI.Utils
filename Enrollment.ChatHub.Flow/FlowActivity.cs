using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.RulesDirector;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Enrollment.ChatHub.Flow
{
    public class FlowActivity(IFlowManager flowManager) : IFlowActivity
    {
        #region Fields
        private readonly IFlowManager _flowManager = flowManager;
        #endregion Fields

        #region Properties
        public DirectorBase Director => _flowManager.Director;
        #endregion Properties

        #region Methods
        public string FormatString(string format, Collection<object> list)
            => FormatString(format, list.ToArray());

        public string FormatString(string format, object[] list)
            => string.Format(CultureInfo.CurrentCulture, format, list);

        public void FlowComplete() => _flowManager.FlowComplete();

        public void Terminate() => _flowManager.Terminate();
        #endregion Methods
    }
}
