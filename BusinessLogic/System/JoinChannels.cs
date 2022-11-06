using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltyAutoTest.BusinessLogic.System
{
    public struct JoinChannels
    {
        public const string CallCenter = "E673F3B9-3C41-407F-B63B-75CAE626990A";
        public const string GiftCardShop = "2A160C20-4117-46B1-B446-CC4B7B7A4C4E";
        public const string TabitKiosk = "1DB24FCC-FA2A-42F3-B53F-68D44B23DEC0";
        public const string Pad = "5F3DE644-E641-45B6-A51B-7F35CAC26D81";
        public const string Office = "7945E45A-A865-43B7-AEB6-5A83728DF9D0";
        public const string PublicApi = "22FA8EB2-3F7E-4CE0-B7AF-A4F9F28E6BCE";
        public const string TabitGuest = "E53A6C41-6C9D-4D7E-8825-F6D18E57A9CA";
        public const string TabitApp = "BF10B302-8152-4E78-B496-7AE7A7513983";
        public const string TabitOrder = "82C4F1A1-9F1F-43F2-A42B-26F3C885734C";
    };

    public class JoinChannel
    {
        public string JoinChannelAlias;
        public string JoinChannelGUID;

        public JoinChannel(string JoinChannelAlias)
        {
            this.JoinChannelAlias = JoinChannelAlias;
            this.JoinChannelGUID = InitJoinChannelGUID();
        }

        private string InitJoinChannelGUID()
        {
            switch (this.JoinChannelAlias)
            {
                case "call-center":
                    return JoinChannels.CallCenter;
                case "gift-card-shop":
                    return JoinChannels.GiftCardShop;
                case "kiosk":
                    return JoinChannels.TabitKiosk;
                case "pad":
                    return JoinChannels.Pad;
                case "office":
                    return JoinChannels.Office;
                case "public-api":
                    return JoinChannels.PublicApi;
                case "tg":
                    return JoinChannels.TabitGuest;
                case "tabit-app":
                    return JoinChannels.TabitApp;
                case "tabit-order":
                    return JoinChannels.TabitOrder;
            }
            throw new Exception("unknown join channel " + this.JoinChannelAlias);
        }
    }
}
