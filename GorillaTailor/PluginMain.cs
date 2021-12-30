using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using PEPlugin;
using PEPlugin.Pmx;
using PEPlugin.SDX;

namespace GorillaTailor
{
    /// <summary>
    /// オプションクラス
    /// </summary>
    internal class GorillaTailorOptions : PEPluginOption
    {
        public GorillaTailorOptions(bool bootup = false, bool regMenu = true, string regMenuText = "") : base(bootup, regMenu, regMenuText)
        {
            Bootup = bootup;
            RegisterMenu = regMenu;
            RegisterMenuText = regMenuText;
        }
        new public bool Bootup { get; private set; }
        new public bool RegisterMenu { get; private set; }
        new public string RegisterMenuText { get; private set; }
    }


    public class PluginMain : IPEPlugin
    {
        #region PEPluginプロパティオーバーライド
        public string Name { get { return "GorillaTailor"; } }

        public string Version
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                return asm.GetName().Version.ToString();
            }
        }

        public string Description { get { return "Tailorロジック検証用"; } }


        public IPEPluginOption Option
        {
            get
            {
#if DEBUG
                return new GorillaTailorOptions(true, true, Name);
#else
                return new GorillaTailorOptions( false, true, Name );
#endif
            }
        }
        #endregion

        private IPEPluginHost Host { get; set; }


        public void Dispose()
        {
            try
            {

            }
            catch (Exception)
            {
                // NOP
            }
        }

        public void Run(IPERunArgs args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("plugin kicked.");

                Host = args.Host;
                //this.verticesDictionary = new Dictionary<IPXVertex, GorillaVertex>();

                // 現在のPMXデータを複製取得
                PEPlugin.Pmx.IPXPmx model = args.Host.Connector.Pmx.GetCurrentState();


                // 解析してボーンを作っちゃえ
                //parseBoneLine(model, args.Host.Connector.Pmx);

                //{
                //    GorillaGenerator generator = new GorillaGenerator();
                //    generator.generateBones(args.Host, model);
                //}
                {
                    GTBoneGenerator generator = new GTBoneGenerator();
                    generator.generateBones(args.Host, model);
                }


                System.Diagnostics.Debug.WriteLine("done.");


                //// モデル名変更
                //pe.ModelInfo.ModelName = "変更しました";

                // 編集したモデル情報でPMXエディタ側を更新
                args.Host.Connector.Pmx.Update(model);

                // エディタ側の表示を更新する場合(一部を除いて表示更新の必要があります | Form更新は旧仕様互換用:Pmd)
                args.Host.Connector.Form.UpdateList(PEPlugin.Pmd.UpdateObject.All);
                args.Host.Connector.View.PmxView.UpdateModel();
            }
            catch (Exception e)
            {
                // NOP
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
    }
}
