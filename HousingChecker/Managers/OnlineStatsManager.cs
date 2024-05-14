using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using HousingChecker.Info;
using System.Threading.Tasks;
using Dalamud.Interface.Internal.Notifications;
using System.Threading;

namespace HousingChecker.Managers;

public class OnlineStatsManager
{
    private static readonly Queue<WardSnapshot> wardQueue = new();
    private static readonly Queue<LotterySnapshot> lotteryQueue = new();
    private readonly TimeSpan uploadInterval = TimeSpan.FromSeconds(5);
    private readonly SemaphoreSlim semaphore = new(0);

    private static readonly HttpClient Client = new();

    private const string BaseUrl0 = "https://house.ffxiv.cyou/api/";
    private const string BaseUrl1 = "https://househelper.ffxiv.cyou/api/";

    internal void Init()
    {
        Client.DefaultRequestHeaders.UserAgent.ParseAdd($"Dalamud.HousingChecker {Plugin.Version} / AtmoOmen");

        if (!string.IsNullOrWhiteSpace(Service.Config.Token))
            Client.DefaultRequestHeaders.Authorization = new("Token", Service.Config.Token.Trim());

        DataUploader();
    }

    private void DataUploader()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await semaphore.WaitAsync();

                if (wardQueue.Count > 0)
                {
                    var wardEntries = new List<WardSnapshot>(wardQueue.Count);
                    lock (wardQueue)
                    {
                        while (wardQueue.Count > 0)
                        {
                            wardEntries.Add(wardQueue.Dequeue());
                        }
                    }
                    await UploadWards(wardEntries);
                }

                if (lotteryQueue.Count > 0)
                {
                    var lotteryEntries = new List<LotterySnapshot>(lotteryQueue.Count);
                    lock (lotteryQueue)
                    {
                        while (lotteryQueue.Count > 0)
                        {
                            lotteryEntries.Add(lotteryQueue.Dequeue());
                        }
                    }
                    await UploadLotteries(lotteryEntries);
                }

                await Task.Delay(uploadInterval);
            }
        });
    }

    public void EnqueueWard(WardSnapshot entry)
    {
        lock (wardQueue)
        {
            wardQueue.Enqueue(entry);
            semaphore.Release();
        }
    }

    public void EnqueueLottery(LotterySnapshot entry)
    {
        lock (lotteryQueue)
        {
            lotteryQueue.Enqueue(entry);
            semaphore.Release();
        }
    }

    private static async Task UploadWards(IReadOnlyCollection<WardSnapshot> entries)
    {
        var response = await UploadDataAsync(entries, "info", "房区信息");
        if (response is { IsSuccessStatusCode: true })
        {
            Service.DalamudNotice.AddNotification(new()
            {
                Title = "HousingChecker",
                Content = $"成功上传了 {entries.Count} 个房区数据!",
                InitialDuration = TimeSpan.FromSeconds(3),
                ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
                Type = NotificationType.Success
            });
        }
    }

    private static async Task UploadLotteries(IReadOnlyCollection<LotterySnapshot> entries)
    {
        var response = await UploadDataAsync(entries, "lottery", "抽选信息");
        if (response is { IsSuccessStatusCode: true })
        {
            Service.DalamudNotice.AddNotification(new()
            {
                Title = "HousingChecker",
                Content = $"成功上传了 {entries.Count} 个抽选数据!",
                InitialDuration = TimeSpan.FromSeconds(3),
                ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(1),
                Type = NotificationType.Success
            });
        }
    }

    private static async Task<HttpResponseMessage> UploadDataAsync<T>(
        IEnumerable<T> data, string endpoint, string contentCategory)
    {
        var baseUrl = BaseUrl0;

        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync($"{baseUrl}{endpoint}", content);
            response.EnsureSuccessStatusCode();

            Service.Log.Debug($"{contentCategory}状态:\n" +
                              $"状态码: {response.StatusCode} 返回内容: {await response.Content.ReadAsStringAsync()}");

            return response;
        }
        catch (HttpRequestException e)
        {
            Service.Log.Error(e, $"上传{contentCategory}失败，尝试备用地址: {baseUrl}");

            // 尝试切换到备用地址
            if (baseUrl == BaseUrl0)
            {
                baseUrl = BaseUrl1;
                Service.Log.Debug($"切换到备用地址: {baseUrl}");
                return await UploadDataAsync(data, endpoint, contentCategory);
            }
            
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

    internal void Uninit() { }
}
