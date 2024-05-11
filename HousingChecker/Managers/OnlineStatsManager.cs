using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace HousingChecker.Managers;

public class OnlineStatsManager
{
    private static readonly HttpClient Client = new();

    private const string BaseUrl = "https://househelper.ffxiv.cyou/api/";

    internal void Init()
    {
        Client.DefaultRequestHeaders.UserAgent.ParseAdd($"Dalamud.HousingChecker {Plugin.Version} / AtmoOmen");

        if (!string.IsNullOrWhiteSpace(Service.Config.Token))
            Client.DefaultRequestHeaders.Authorization = new("Token", Service.Config.Token.Trim());
    }

    public async void UploadWard(object entry)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(entry), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync($"{BaseUrl}info", content);

            Service.Log.Debug($"上传状态:\n" +
                              $"StatusCode: {response.StatusCode} Content: {response.Content.ReadAsStringAsync().Result}");
        }
        catch (Exception e)
        {
            Service.Log.Error(e, "Upload failed");
            Service.Chat.PrintError($"上传失败: {e.Message}");
        }
    }

    internal void Uninit()
    {

    }
}
