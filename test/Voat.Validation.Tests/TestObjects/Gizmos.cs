using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Voat.Validation.Tests
{
    [DataValidation(typeof(GizmoValidator))]
    public class Gizmo
    {
        public string ID { get; set; }
    }

    public class GizmoValidator : DataValidator<Gizmo>
    {
        public override IEnumerable<ValidationPathResult> Validate(Gizmo value, ValidationContext context)
        {
            if (String.IsNullOrEmpty(value.ID))
            {
                return ValidationPathResult.Create(value, "{0} is empty", "Required", x => x.ID).ToEnumerable();
            }
            return null;
        }
    }

    [DataValidation(typeof(MetalGizmoValidator))]
    public class MetalGizmo : Gizmo
    {
        public string Alloy { get; set; }
    }
    public class MetalGizmoValidator : DataValidator<MetalGizmo>
    {
        public override IEnumerable<ValidationPathResult> Validate(MetalGizmo value, ValidationContext context)
        {
            if (String.IsNullOrEmpty(value.Alloy))
            {
                return ValidationPathResult.Create(value, "{0} is empty", "Required", x => x.Alloy).ToEnumerable();
            }
            return null;
        }
    }
}