﻿using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System;
using System.Linq;
using System.Threading.Tasks;
using TravBotSharp.Files.Helpers;
using TravBotSharp.Files.Models.AccModels;

namespace TravBotSharp.Files.Tasks.LowLevel
{
    public class TTWarsGetRes : BotTask
    {
        public override async Task<TaskRes> Execute(HtmlDocument htmlDoc, ChromeDriver wb, Files.Models.AccModels.Account acc)
        {
            await acc.Wb.Navigate($"{acc.AccInfo.ServerUrl}/dorf2.php");

            Random rnd = new Random();
            int sec = rnd.Next(370, 380);
            TaskExecutor.AddTask(acc, new TTWarsGetRes() { ExecuteAt = DateTime.Now.AddSeconds(sec), vill = AccountHelper.GetMainVillage(acc) });
            TaskExecutor.AddTask(acc, new TrainExchangeRes() { ExecuteAt = DateTime.Now.AddSeconds(sec + 5), troop = acc.Villages[0].Troops.TroopToTrain ?? Classificator.TroopsEnum.Hero, vill = vill });
            TaskExecutor.AddTask(acc, new TrainTroops()
            {
                ExecuteAt = DateTime.Now.AddSeconds(sec + 9),
                Troop = acc.Villages[0].Troops.TroopToTrain ?? Classificator.TroopsEnum.Hero,
                vill = vill,
                HighSpeedServer = true
            });


            wb.ExecuteScript("window.fireEvent('startPaymentWizard', {data:{activeTab: 'paymentFeatures'}});");

            await Task.Delay(AccountHelper.Delay() * 2);

            wb.ExecuteScript("$$('.paymentWizardMenu').addClass('hide');$$('.buyGoldInfoStep').removeClass('active');$$('.buyGoldInfoStep#2').addClass('active');$$('.paymentWizardMenu#buyResources').removeClass('hide');"); //Excgabge resources button

            await Task.Delay(AccountHelper.Delay() * 2);

            htmlDoc.LoadHtml(wb.PageSource);

            //gold prosButton buyResources6
            //gold prosButton buyAnimal5
            var buy = htmlDoc.DocumentNode.Descendants("button").FirstOrDefault(x => x.HasClass("buyResources6"));
            if (buy == null)
            {
                this.ErrorMessage = "Can't find the button with class buyResources6. Are you sure you are on vip/unl TTWars server?";
                return TaskRes.Executed;
            }
            var buyId = buy.GetAttributeValue("id", "");
            wb.ExecuteScript($"document.getElementById('{buyId}').click()");
            return TaskRes.Executed;
        }
    }
}
