using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Domain.Models
{
    public class GenericReference<T> where T: struct
    {
        public string Name { get; set; }
        public T Type { get; set; }
    }
}
