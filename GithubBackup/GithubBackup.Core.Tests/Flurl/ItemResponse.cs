﻿using System.Text.Json.Serialization;

namespace GithubBackup.Core.Tests.Flurl;

public class ItemResponse
{
    [JsonPropertyName("items")]
    public List<Item> Items { get; }

    public ItemResponse(List<Item> items)
    {
        Items = items;
    }
}