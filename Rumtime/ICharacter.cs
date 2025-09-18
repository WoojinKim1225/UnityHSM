using System.Collections.Generic;
using UnityEngine;

namespace HSM
{
    public interface ICharacter
    {
        public System.Type[] states { get; set; }
    }
}