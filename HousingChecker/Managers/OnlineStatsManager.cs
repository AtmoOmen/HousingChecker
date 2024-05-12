using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using HousingChecker.Info;
using System.Threading.Tasks;
using Dalamud.Interface.Internal.Notifications;
using HousingChecker.Helpers;
using Lumina.Excel.GeneratedSheets2;

namespace HousingChecker.Managers;

public class OnlineStatsManager
{
    private static readonly HttpClient Client = new();

    private const string BaseUrl = "https://house.ffxiv.cyou/api/";

    internal void Init()
    {
        Client.DefaultRequestHeaders.UserAgent.ParseAdd($"Dalamud.HousingChecker {Plugin.Version} / AtmoOmen");

        if (!string.IsNullOrWhiteSpace(Service.Config.Token))
            Client.DefaultRequestHeaders.Authorization = new("Token", Service.Config.Token.Trim());
    }

    public async void UploadWard(WardSnapshot entry)
    {
        var response = await UploadDataAsync([entry], "info", "房区信息");
        if (response is { IsSuccessStatusCode: true })
        {
            var serverName = Service.Data.GetExcelSheet<World>().GetRow((uint)entry.server).Name.RawString;
            Service.DalamudNotice.AddNotification(new()
            {
                Title = "HousingChecker",
                Content = $"房区信息: {entry.area} {entry.slot + 1}区 ({serverName}) 上传成功!",
                InitialDuration = TimeSpan.FromSeconds(3),
                ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
                Type = NotificationType.Success
            });
        }
    }

    public async void UploadLottery(LotterySnapshot entry)
    {
        var response = await UploadDataAsync([entry], "lottery", "抽选信息");
        if (response is { IsSuccessStatusCode: true })
        {
            var serverName = Service.Data.GetExcelSheet<World>().GetRow((uint)entry.ServerId).Name.RawString;
            Service.DalamudNotice.AddNotification(new()
            {
                Title = "HousingChecker",
                Content = $"抽选信息: {Utils.HouseAreaNumberToString(entry.Area)} {entry.Slot + 1}区{entry.LandID}号 ({serverName}) 上传成功!",
                InitialDuration = TimeSpan.FromSeconds(3),
                ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
                Type = NotificationType.Success
            });
        }
    }

    private static async Task<HttpResponseMessage?> UploadDataAsync<T>(IEnumerable<T> data, string endpoint, string contentCategory)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync($"{BaseUrl}{endpoint}", content);

            Service.Log.Debug($"{contentCategory}状态:\n" +
                              $"状态码: {response.StatusCode} 返回内容: {response.Content.ReadAsStringAsync().Result}");

            return response;
        }
        catch (Exception e)
        {
            Service.Log.Error(e, $"上传{contentCategory}失败");
            Service.DalamudNotice.AddNotification(new()
            {
                Title = "HousingChecker",
                Content = $"上传{contentCategory}失败: {e.Message}",
                InitialDuration = TimeSpan.FromSeconds(5),
                ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
                Type = NotificationType.Error
            });

            return null;
        }
    }

    public async Task<List<HousingSellInfo>?> DownloadDataAsync(uint serverID)
    {
        try
        {
            var content = await Client.GetStringAsync($"https://house.ffxiv.cyou/api/sales?server={serverID}");
            return JsonConvert.DeserializeObject<List<HousingSellInfo>?>(content);
        }
        catch (Exception e)
        {
            Service.Log.Error(e, "尝试拉取在线信息失败");
            Service.DalamudNotice.AddNotification(new()
            {
                Title = "HousingChecker",
                Content = $"拉取在线信息失败: {e.Message}",
                InitialDuration = TimeSpan.FromSeconds(5),
                ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
                Type = NotificationType.Error
            });

            return null;
        }
    }


    internal void Uninit()
    {

    }
}
