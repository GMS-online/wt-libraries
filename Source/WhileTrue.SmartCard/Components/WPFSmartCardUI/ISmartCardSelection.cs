﻿using WhileTrue.Classes.Components;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.SmartCardUI
{
    [ComponentInterface]
    public interface ISmartCardSelection
    {
        ICardReader ShowModal(ISmartCardService smartCardService, bool acceptEmptyCardReader, string details);
    }
}