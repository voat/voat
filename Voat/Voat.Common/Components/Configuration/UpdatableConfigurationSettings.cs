using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Common.Configuration
{
    public interface IHandlesConfigurationUpdate<T>
    {
        void Update(T configSettings);
    }

    public abstract class UpdatableConfigurationSettings<T> : ConfigurationSettings<T>, IHandlesConfigurationUpdate<T> where T : UpdatableConfigurationSettings<T>, new()
    {
        protected EventHandler _updateHandler;

        public event EventHandler OnUpdate
        {
            add
            {
                _updateHandler += value;
            }
            remove
            {
                _updateHandler -= value;
            }
        }

        public virtual void Update(T configInstance)
        {
            //TODO: Figure out how to copy this instance over to the new one with event handlers in place
            //Copy to Instance -- this is completely untested and it smells
            configInstance._updateHandler = Instance._updateHandler;
            Instance = configInstance;

            _updateHandler?.Invoke(this, EventArgs.Empty);
        }
    }
}
