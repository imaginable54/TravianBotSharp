﻿using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System;
using System.Linq;
using System.Threading.Tasks;
using TravBotSharp.Files.Helpers;
using TravBotSharp.Files.Models.AccModels;

namespace TravBotSharp.Files.Tasks.LowLevel
{
    public class TrainExchangeRes : BotTask
    {
        public bool Great { get; set; }
        public Classificator.TroopsEnum troop { get; set; }

        public override async Task<TaskRes> Execute(HtmlDocument htmlDoc, ChromeDriver wb, Files.Models.AccModels.Account acc)
        {
            if (vill == null) vill = AccountHelper.GetMainVillage(acc);

            Classificator.BuildingEnum building = (Great == false) ? TroopsHelper.GetTroopBuilding(troop, false) : TroopsHelper.GetTroopBuilding(troop, true);

            var buildId = vill.Build.Buildings.FirstOrDefault(x => x.Type == building);
            if (buildId == null)
            {
                //update dorf, no buildingId found?
                TaskExecutor.AddTask(acc, new UpdateDorf2() { ExecuteAt = DateTime.Now });
                Console.WriteLine($"There is no {building} in this village!");
                return TaskRes.Executed;
            }
            await acc.Wb.Navigate($"{acc.AccInfo.ServerUrl}/build.php?id={buildId.Id}");

            var troopNode = htmlDoc.DocumentNode.Descendants("img").FirstOrDefault(x => x.HasClass("u" + (int)troop));
            while (!troopNode.HasClass("details")) troopNode = troopNode.ParentNode;

            //finding the correct "Exchange resources" button
            var exchangeResButton = troopNode.Descendants("button").FirstOrDefault(x => x.HasClass("gold"));

            wb.ExecuteScript($"document.getElementById('{exchangeResButton.GetAttributeValue("id", "")}').click()"); //Exchange resources button

            await Task.Delay(AccountHelper.Delay());
            htmlDoc.LoadHtml(wb.PageSource);
            await Task.Delay(AccountHelper.Delay());

            var distribute = htmlDoc.DocumentNode.SelectNodes("//*[text()[contains(., 'Distribute remaining resources.')]]")[0];
            while (distribute.Name != "button") distribute = distribute.ParentNode;
            string distributeid = distribute.GetAttributeValue("id", "");
            wb.ExecuteScript($"document.getElementById('{distributeid}').click()"); //Distribute resources button

            await Task.Delay(AccountHelper.Delay());
            wb.ExecuteScript($"document.getElementById('npc_market_button').click()"); //Exchange resources button

            return TaskRes.Executed;
        }
    }
}
