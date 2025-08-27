using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Dialogs
{
    public class Dialog_BigConfirm : Dialog_Confirm
    {
        private Vector2 _size;
        public override Vector2 InitialSize => _size;

        public Dialog_BigConfirm(string title, Action onConfirm, Vector2 size) : base(title, onConfirm)
        {
            _size = size;
        }

        public Dialog_BigConfirm(string title, Action onConfirm) : this(title, onConfirm, new Vector2(512, 256))
        {
        }
    }
}
