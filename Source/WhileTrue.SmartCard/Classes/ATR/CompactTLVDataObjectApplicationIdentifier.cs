using System;

namespace WhileTrue.Classes.ATR
{
    public class CompactTLVDataObjectApplicationIdentifier : CompactTLVDataObjectBase
    {
        /*
         http://www.emvlab.org/emvtags/?number=4F
         http://www.kartenbezogene-identifier.de/de/rapi/rid-liste.html
        */
        public CompactTLVDataObjectApplicationIdentifier(AtrCompactTlvHistoricalCharacters owner):base(owner)
        {
        }
        private byte[] aid;

        protected override byte[] GetValue()
        {
            return this.aid;
        }

        protected override void UpdateValue(byte[] data)
        {
            if (data != null)
            {
                this.IsApplicable = true;
                this.Aid = data;
            }
            else
            {
                this.IsApplicable = false;
            }
        }

        public override CompactTLVTypes Type
        {
            get { return CompactTLVTypes.ApplicationIdentifer; }
        }

        protected override byte[] GetDefaultValue()
        {
            return new byte[]{0xF0};
        }

        public byte[] Aid
        {
            get { return this.aid; }
            set
            {
                if (value.Length <= 15)
                {
                    this.SetAndInvoke(() => Aid, ref this.aid, value);
                    this.NotifyChanged();
                }
                else
                {
                    throw new ArgumentException("Aid must have max. 15 bytes","value");
                }
            }
        }
    }
}