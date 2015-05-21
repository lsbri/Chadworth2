using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace KurisuBlitz
{
    //  _____ _ _ _                       _   
    // | __  | |_| |_ ___ ___ ___ ___ ___| |_ 
    // | __ -| | |  _|- _|  _|  _| .'|   | '_|
    // |_____|_|_|_| |___|___|_| |__,|_|_|_,_|
    //  Copyright © Kurisu Solutions 2015

    internal class Program
    {
        private static Menu _menu;
        private static Spell _q, _w, _e;
        private static Orbwalking.Orbwalker _orbwalker;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;

        static void Main(string[] args)
        {
            Console.WriteLine("Chads Rengar injected..");
            CustomEvents.Game.OnGameLoad += Rengonload;
        }

        private static void Rengonload(EventArgs args)
        {
            if (Me.ChampionName != "Rengar")
                return;

            // Set spells      
            _q = new Spell(SpellSlot.Q, 0f);
            _w = new Spell(SpellSlot.W, 500f); 
            _e = new Spell(SpellSlot.E, 950f);
            _e.SetSkillshot(250f, 75f, 1800f, true, SkillshotType.SkillshotLine);
            

            // Load Menu
            _menu = new Menu("RengoChad", "Rengar", true);

            var RengarTs = new Menu("Selector", "tselect");
            TargetSelector.AddToMenu(RengarTs);
            _menu.AddSubMenu(RengarTs);

            var RengarOrb = new Menu("Orbwalker", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(RengarOrb);
            _menu.AddSubMenu(RengarOrb);

            var menuD = new Menu("Drawings", "drawings");
            menuD.AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            menuD.AddItem(new MenuItem("drawE", "Draw E")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            menuD.AddItem(new MenuItem("drawT", "Draw Target")).SetValue(true);
            _menu.AddSubMenu(menuD);

            var kkmenu = new Menu("Keybinds", "keybinds");
            kkmenu.AddItem(new MenuItem("combokey", "Combo (active)")).SetValue(new KeyBind(32, KeyBindType.Press));
            _menu.AddSubMenu(kkmenu);

            var spellmenu = new Menu("SpellMenu", "smenu");

            var menuQ = new Menu("Q Menu", "qmenu");
            menuQ.AddItem(new MenuItem("usecomboq", "Use in Combo")).SetValue(true);
            menuQ.AddItem(new MenuItem("secureq", "Use for Killsteal")).SetValue(false);
            spellmenu.AddSubMenu(menuQ);

            var menuE = new Menu("W Menu", "emenu");
            menuE.AddItem(new MenuItem("usecomboe", "Use in Combo")).SetValue(true);
            menuE.AddItem(new MenuItem("securee", "Use for Killsteal")).SetValue(false);
            spellmenu.AddSubMenu(menuE);

            var menuR = new Menu("E Menu", "rmenu");
            menuR.AddItem(new MenuItem("usecombor", "Use in Combo")).SetValue(true);
            menuR.AddItem(new MenuItem("securer", "Use for Killsteal")).SetValue(false);
            spellmenu.AddSubMenu(menuR);

            // events
            Drawing.OnDraw += RengaOnDraw;
            Game.OnUpdate += RengarOnUpdate;

            Game.PrintChat("<font color=\"#FF9900\"><b>KurisuBlitz:</b></font> Loaded");

        }

        private static void RengarOnUpdate(EventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void RengaOnDraw(EventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void RengoOnDraw(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_q.Range * 2, TargetSelector.DamageType.Physical);

            if (!Me.IsDead)
            {
                var rcircle = _menu.Item("drawR").GetValue<Circle>();
                var qcircle = _menu.Item("drawQ").GetValue<Circle>();

                if (qcircle.Active)
                    Render.Circle.DrawCircle(Me.Position, _q.Range, qcircle.Color);

                if (rcircle.Active)
                    Render.Circle.DrawCircle(Me.Position, _w.Range, qcircle.Color);

                if (target.IsValidTarget(_q.Range * 2) && _menu.Item("drawT").GetValue<bool>())
                    Render.Circle.DrawCircle(target.Position, target.BoundingRadius - 30, Color.Yellow, 3);
            }
        }

        private static void BlitzOnUpdate(EventArgs args)
        {
            // kill secure
            Secure(_menu.Item("secureq").GetValue<bool>(), _menu.Item("securee").GetValue<bool>(),
                   _menu.Item("securer").GetValue<bool>());

            // auto grab
            AutoCast(_menu.Item("qdashing").GetValue<bool>(),
                     _menu.Item("qimmobile").GetValue<bool>());

            if ((int)(Me.Health / Me.MaxHealth * 100) >= _menu.Item("hnd").GetValue<Slider>().Value)
            {
                if (_menu.Item("combokey").GetValue<KeyBind>().Active)
                {
                    Combo(_menu.Item("usecomboq").GetValue<bool>(),
                          _menu.Item("usecomboe").GetValue<bool>());
                }
            }
        }

        private static void AutoCast(bool dashing, bool immobile)
        {
            if (_q.IsReady())
            {
                foreach (var itarget in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(_q.Range)))
                {
                    if (dashing && _menu.Item("dograb" + itarget.ChampionName).GetValue<StringList>().SelectedIndex == 2)
                        if (itarget.Distance(Me.ServerPosition) > _menu.Item("dnd").GetValue<Slider>().Value)
                            _q.CastIfHitchanceEquals(itarget, HitChance.Dashing);

                    if (immobile && _menu.Item("dograb" + itarget.ChampionName).GetValue<StringList>().SelectedIndex == 2)
                        if (itarget.Distance(Me.ServerPosition) > _menu.Item("dnd").GetValue<Slider>().Value)
                            _q.CastIfHitchanceEquals(itarget, HitChance.Immobile);
                }
            }

            if (_r.IsReady())
            {
                foreach (var rtarget in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(_r.Range)))
                {
                    if (rtarget.IsValidTarget(_r.Range) && _menu.Item("usecombor").GetValue<bool>())
                    {
                        if (!_e.IsReady() && rtarget.HasBuffOfType(BuffType.Knockup))
                            _r.Cast();
                    }
                }
            }
        }

        private static void Combo(bool useq, bool usee)
        {
            if (useq && _q.IsReady())
            {
                var qtarget = TargetSelector.GetTargetNoCollision(_q);
                if (qtarget.IsValidTarget(_q.Range))
                {
                    var poutput = _q.GetPrediction(qtarget);
                    if (poutput.Hitchance >= (HitChance)_menu.Item("hitchanceq").GetValue<Slider>().Value + 2)
                    {
                        if (qtarget.Distance(Me.ServerPosition) > _menu.Item("dnd").GetValue<Slider>().Value)
                        {
                            if (_menu.Item("dograb" + qtarget.ChampionName).GetValue<StringList>().SelectedIndex != 0)
                                _q.Cast(poutput.CastPosition);
                        }
                    }
                }
            }

            if (usee && _e.IsReady())
            {
                var etarget = TargetSelector.GetTarget(250, TargetSelector.DamageType.Physical);
                if (etarget.IsValidTarget(_e.Range + 100))
                {
                    if (_menu.Item("usecomboe").GetValue<bool>() && !_q.IsReady())
                        _e.CastOnUnit(Me);
                }
            }
        }

        private static void Secure(bool useq, bool usee, bool user)
        {
            if (useq && _q.IsReady())
                if (Player.Mana <= 5)
            {
            
                var qtarget = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.IsEnemy);
                if (qtarget.IsValidTarget(_q.Range))
                {
                    if (Me.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                        _q.CastOnUnit(Me);
                }
            }

                        {
            if (user && _w.IsReady())

            {
                var rtarget = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.IsEnemy);
                if (rtarget.IsValidTarget(_w.Range))
                {
                    if (Me.GetSpellDamage(rtarget, SpellSlot.W) >= rtarget.Health)
                        _w.Cast();
                }
            }


            if (usee && _e.IsReady())
            {
                var etarget = TargetSelector.GetTargetNoCollision(_q);
                if (etarget.IsValidTarget(_e.Range))
                {
                    var poutput = _q.GetPrediction(etarget);
                    if (poutput.Hitchance >= (HitChance) _menu.Item("hitchanceq").GetValue<Slider>().Value + 2)
                    {
                        if (etarget.Distance(Me.ServerPosition) > _menu.Item("dnd").GetValue<Slider>().Value)
                        {
                            if (_menu.Item("usecombo" + etarget.ChampionName).GetValue<StringList>().SelectedIndex != 0) 
                                _q.Cast(poutput.CastPosition);
                        }
                    }
                }
            }

            }
                            }
                        }

    internal class Player
    {
        public static int Mana { get; set; }
    }
}
                
