﻿using SlipeServer.Resources.Text3d;

namespace SlipeServer.Console;

internal class TestLogic
{
    private readonly Text3dService _text3DService;

    public TestLogic(Text3dService text3DService)
    {
        _text3DService = text3DService;
        var textId = _text3DService.CreateText3d(new System.Numerics.Vector3(5, 0, 4), "Here player spawns");
        Task.Run(async () =>
        {
            while(true)
            {
                await Task.Delay(1000);
                _text3DService.UpdateText3d(textId, $"Current date time: {DateTime.Now}");
            }
        });

        var textId2 = _text3DService.CreateText3d(new System.Numerics.Vector3(10, 0, 4), "Destroyed text 3d");
        _text3DService.RemoveText3d(textId2);
    }
}
