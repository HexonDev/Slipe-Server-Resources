using SlipeServer.Server;
using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Resources.DxProxy
{
    public class DxProxyLogic
    {
        private readonly LuaEventService luaEventService;
        private readonly DxProxyResource resource;

        public DxProxyLogic(MtaServer server, LuaEventService luaEventService)
        {
            this.luaEventService = luaEventService;
            this.resource = server.GetAdditionalResource<DxProxyResource>();

            server.PlayerJoined += HandlePlayerJoin;
        }

        private void HandlePlayerJoin(Server.Elements.Player player)
        {
            this.resource.StartFor(player);
        }
    }
}
