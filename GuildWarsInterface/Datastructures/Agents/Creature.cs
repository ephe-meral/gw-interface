﻿#region

using GuildWarsInterface.Datastructures.Agents.Components;
using GuildWarsInterface.Datastructures.Items;
using GuildWarsInterface.Declarations;
using GuildWarsInterface.Misc;
using GuildWarsInterface.Networking;
using GuildWarsInterface.Networking.Protocol;

#endregion

namespace GuildWarsInterface.Datastructures.Agents
{
        public abstract class Creature : Agent
        {
                public readonly CreatureBuffs Buffs;
                public readonly CreatureEnergy Energy;
                public readonly CreatureHealth Health;
                public readonly Inventory Inventory;

                private byte _level;
                private uint _morale;
                private Professions _professions;
                private CreatureStatus _status;

                protected Creature()
                {
                        Morale = 100;
                        Level = 1;
                        Health = new CreatureHealth(this);
                        Energy = new CreatureEnergy(this);
                        Buffs = new CreatureBuffs(this);
                        Professions = new Professions(Profession.Warrior, Profession.None);
                        Inventory = new Inventory();
                }

                public CreatureStatus Status
                {
                        get { return _status; }
                        set
                        {
                                _status = value;

                                if (Created)
                                {
                                        Network.GameServer.Send(GameServerMessage.AgentStatus, IdManager.GetId(this), (uint) Status);
                                }
                        }
                }

                public byte Level
                {
                        get { return _level; }
                        set
                        {
                                _level = value;

                                if (Created)
                                {
                                        SendAgentPropertyInt(AgentProperty.Level, Level);
                                }
                        }
                }


                public Professions Professions
                {
                        get { return _professions; }
                        set
                        {
                                _professions = value;

                                if (Created)
                                {
                                        Network.GameServer.Send(GameServerMessage.AgentProfessions,
                                                                IdManager.GetId(this),
                                                                (byte) _professions.Primary,
                                                                (byte) _professions.Secondary);
                                }
                        }
                }


                public uint Morale
                {
                        get { return _morale; }
                        set
                        {
                                _morale = value;

                                if (Created)
                                {
                                        Network.GameServer.Send(GameServerMessage.AgentMorale, IdManager.GetId(this), Morale);
                                }
                        }
                }

                public void PerformAnimation(CreatureAnimation animation, bool loop = false)
                {
                        if (Game.State == GameState.Playing)
                        {
                                SendAgentPropertyInt(loop ? AgentProperty.AnimationLooped : AgentProperty.Animation, (uint) animation);
                        }
                }

                private void PerformEmote(AgentProperty property, uint value)
                {
                        if (Game.State == GameState.Playing)
                        {
                                SendAgentPropertyInt(property, value);
                                PerformAnimation(CreatureAnimation.None);
                        }
                }

                public void PerformFameEmote(uint rank)
                {
                        PerformEmote(AgentProperty.FameEmote, rank);
                }

                public void PerformZaishenEmote(uint rank)
                {
                        PerformEmote(AgentProperty.ZaishenEmote, rank);
                }

                public void PerformWingsEmote(bool white = false)
                {
                        PerformEmote(AgentProperty.WingsEmote, (uint) (white ? 2 : 1));
                }

                public void CastInstant(Skill skill, Agent target)
                {
                        SendAgentTargetPropertyInt(AgentProperty.CastInstant, target, (uint) skill);
                }

                public void CastSkill(Skill skill, float castTime, Agent target)
                {
                        SendAgentPropertyFloat(AgentProperty.CastTimeModifier, castTime);
                        SendAgentTargetPropertyInt(AgentProperty.CastSkill, target, (uint) skill);
                }

                public void CancelSkill()
                {
                        SendAgentPropertyInt(AgentProperty.CancelSkill, 0);
                }

                public void CastAttack(Skill skill, float castTime, Agent target)
                {
                        SendAgentPropertyFloat(AgentProperty.CastTimeModifier, castTime);
                        SendAgentTargetPropertyInt(AgentProperty.CastAttack, target, (uint) skill);
                }

                public void CancelAttack()
                {
                        SendAgentPropertyInt(AgentProperty.CancelAttack, 0);
                }

                public void SpeechBubble(string text)
                {
                        Network.GameServer.Send(GameServerMessage.SpeechBubble,
                                                IdManager.GetId(this),
                                                new HString(text).Serialize());
                }

                public void ShootProjectile(Position targetPosition, float flightTime, uint effect)
                {
                        Network.GameServer.Send(GameServerMessage.Projectile,
                                                IdManager.GetId(this),
                                                targetPosition.X,
                                                targetPosition.Y,
                                                targetPosition.Plane,
                                                flightTime,
                                                effect,
                                                1, // unknown
                                                (byte) 0); // ignored
                }
        }
}