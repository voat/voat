using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Common.Configuration
{
    public interface IHandlesConfigurationUpdate<T>
    {
        void Update(T newSettings);
    }

    public abstract class UpdatableConfigurationSettings<T> : ConfigurationSettings<T>, IHandlesConfigurationUpdate<T> where T : UpdatableConfigurationSettings<T>, new()
    {
        protected EventHandler<T> _updateHandler;
        protected TimeSpan _duplicateUpdateDelay = TimeSpan.FromSeconds(1);
        protected DateTime _lastUpdated = DateTime.UtcNow;

        public event EventHandler<T> OnUpdate
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

        public virtual void Update(T newSettings)
        {
            if (DateTime.UtcNow.Subtract(_lastUpdated) > _duplicateUpdateDelay)
            {
                //TODO: Figure out how to copy this instance over to the new one with event handlers in place
                //Copy to Instance -- this is completely untested and it smells
                newSettings._updateHandler = Instance._updateHandler;
                Instance = newSettings;

                _updateHandler?.Invoke(this, newSettings);

            }
        }
    }
}
