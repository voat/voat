using System;
using System.Reflection;
using System.Web.Mvc;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using Ploeh.AutoFixture.Xunit2;
using Voat.Models;

namespace Voat.Services.UnitTests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class VoatTestAttribute : AutoDataAttribute 
    {
        public VoatTestAttribute()
            : base(new Fixture().Customize(
                new TestableVoat()))
        {
            
        }
    }

    public class TestableVoat : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new IgnoreVirtualMembers());

            Effort.Provider.EffortProviderConfiguration.RegisterProvider();

            fixture.Register(() => new whoaverseEntities(Effort.EntityConnectionFactory.CreateTransient("metadata=res://*/Models.WhoaverseEntityDataModel.csdl|res://*/Models.WhoaverseEntityDataModel.ssdl|res://*/Models.WhoaverseEntityDataModel.msl;provider=System.Data.SqlClient;"), true));           
        }
    }

    public class IgnoreVirtualMembers : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var pi = request as PropertyInfo;
            if (pi == null)
            {
                return new NoSpecimen(request);
            }

            if (pi.GetGetMethod().IsVirtual)
            {
                return null;
            }
            return new NoSpecimen(request);
        }
    }
}
