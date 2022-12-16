using MethodDecorator.Fody.Interfaces;
using NLog;
using System.Reflection;

namespace Wada.AOP.Logging
{
    // Any attribute which provides OnEntry/OnExit/OnException with proper args
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
    public class LoggingAttribute : Attribute, IMethodDecorator
    {
        private object? _instance;
        private MethodBase? _method;
        private object[]? _args;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        // instance, method and args can be captured here and stored in attribute instance fields
        // for future usage in OnEntry/OnExit/OnException

        public void Init(object instance, MethodBase method, object[] args)
        {
            _instance = instance;
            _method = method;
            _args = args;
        }

        public void OnEntry()
        {
            string ar = _args == null ? string.Empty : string.Join(",", _args);
            _logger.Trace($"Executing Class {_instance}, Method {_method?.Name}({ar})");
        }

        public void OnExit()
        {
            string ar = _args == null ? string.Empty : string.Join(",", _args);
            _logger.Trace($"Executed Class {_instance}, Method {_method?.Name}({ar})");
        }

        public void OnException(Exception exception)
        {
            string ar = _args == null ? string.Empty : string.Join(",", _args);
            _logger.Error(exception, $"Threw Exception:{exception.Message}, Class {_instance}, Method {_method?.Name}({ar})");
        }
    }
}
