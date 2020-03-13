#define DEBUG_CORE


using System;
using System.Collections.Generic;
using System.Text;
using SNet;
using Core.Net;
using System.IO;
using System.Linq;
using Sio;
using UnityEngine;
using XLua;
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;

[LuaCallCSharp()]
[CSharpCallLua]
public class net_process : SNet.NIprocess
{
	private static object lockobject = new object();
	private static net_process instance = null;
#if UNITY_EDITOR

	#region 基本协议
	private static string[] all_handle_message = {
                                            "msg_client_log.cg_record_auto_set_effect",
                                            "msg_client_log.cg_get_auto_set_effect",
                                            "client_proxy.cp_enter_game_no_auth",
                                            "system.cg_enter_game",
                                            "system.cg_gm_cmd",
                                            "system.cg_add_guide_log",
                                            "system.cg_cheater_check",
                                            "player.cg_rand_name",
                                            "player.cg_create_player_info",
                                            "player.cg_set_peekshow",
                                            "player.cg_first_enter_game_complete",
                                            "player.cg_guide_id",
                                            "player.cg_get_month_cards_state",
                                            "msg_chat.cg_cache_chat",
                                            "msg_store.cg_request_store_data",
                                            "msg_checkin.cg_get_checkin_info",
                                            "msg_checkin.cg_get_month_sign_state",
                                            "msg_checkin.cg_sign_in_c_day",
                                            "msg_activity.cg_activity_request_state",
                                            "msg_activity.cg_login_request_my_data",
                                            "msg_activity.cg_hurdle_request_my_data",
                                            "msg_activity.cg_get_level_up_reward_data",
                                            "msg_activity.cg_get_everyday_recharge_data",
                                            "msg_activity.cg_niudan_request_role_info",
                                            "msg_activity.cg_niudan_use",
                                            "msg_activity.cg_login_get_reward",
                                            "msg_activity.cg_get_level_fund_state",
                                            "msg_activity.cg_arena_request_myslef_info",
                                            "msg_activity.cg_arena_request_player_list",
                                            "msg_activity.cg_enter_activity",
                                            "msg_activity.cg_leave_activity",
                                            "msg_activity.cg_arena_request_climb_reward_data",
                                            "msg_activity.cg_arena_get_climb_reward",
                                            "msg_activity.cg_arena_get_day_point_reward",
                                            "msg_activity.cg_share_activity_state",
                                            "msg_activity.cg_get_recruit_states",
                                            "msg_shop.cg_shop_item_info",
                                            "msg_hurdle.cg_hurdle_fight",
                                            "msg_hurdle.cg_hurdle_fight_result",
                                            "msg_hurdle.cg_take_award",
                                            "msg_hurdle.cg_hurdle_saodang",
                                            "msg_hurdle.cg_hurlde_box",
                                            "msg_hurdle.cg_attribute_verify_start",
                                            "msg_hurdle.cg_attribute_verify_create_hero",
                                            "msg_hurdle.cg_attribute_verify_upload",
                                            "msg_hurdle.cg_attribute_verify_change_info",
                                            "msg_hurdle.cg_attribute_verify_over",
                                            "msg_hurdle.cg_open_new_group_animation",
                                            "msg_team.cg_update_team_info",
                                            "msg_cards.cg_hero_rarity_up",
                                            "msg_cards.cg_hero_star_up",
                                            "msg_cards.cg_eat_exp",
                                            "msg_cards.cg_skill_level_up",
                                            "msg_cards.cg_soul_exchange_hero",
                                            "msg_cards.cg_equip_level_up",
                                            "msg_cards.cg_equip_rarity_up",
                                            "msg_dailytask.cg_request_my_dailytask_info",
                                            "msg_dailytask.cg_request_dailytask_list",
                                            "msg_dailytask.cg_finish_task",
                                            "msg_dailytask.cg_on_line_task_state",
                                            "msg_dailytask.cg_line_finish_task",
                                            "msg_dailytask.cg_line_finish_task_all",
                                            "msg_sign_in.cg_request_task_list",
                                            "msg_sign_in.cg_get_award",
                                            "msg_rank.cg_rank",
                                            "msg_weaknet_white.cg_ping",
                                            "msg_mail.cg_get_maildata",
                                            "msg_activity.cg_niudan_request_equip_info",
                                            "msg_shop.cg_buy_shop_item",
                                            "msg_sign_in.cg_request_total_state",
                                            "msg_activity.cg_arena_clean_cd",
                                            "player.cg_select_country",
                                            "msg_shop.cg_sell_item_for_sell",
                                            "msg_activity.cg_kuikuliya_request_all_floor_data",
                                            "msg_activity.cg_request_kuikuliya_myself_data",
                                            "msg_activity.cg_kuikuliya_get_box_reward",
                                            "msg_activity.cg_reset_kuikuliya",
                                            "msg_activity.cg_saodang_kuikuliya",
                                            "msg_expedition_trial.cg_request_expedition_trial_info",
                                            "msg_expedition_trial.cg_challenge_expedition_trial",
                                            "msg_cards.cg_set_card_play_method_cur_hp",
                                            "msg_expedition_trial.cg_expedition_trial_challenge_result",
                                            "msg_expedition_trial.cg_get_expedition_trial_points_reward",
                                            "msg_expedition_trial.cg_trigger_expedition_trial_level",
                                            "msg_activity.cg_kuikuliya_Get_saodang_reward",
                                            };
	

	#endregion

    private static Dictionary<string, int> m_allHandleMessage = new Dictionary<string, int>();
#endif
    public static net_process GetInstance()
	{
		lock (lockobject)
		{
			if (instance == null)
            {
				instance = new net_process();
#if UNITY_EDITOR
                for (int i = 0; i < all_handle_message.Length; ++i)
                    m_allHandleMessage.Add(all_handle_message[i], 1);
#endif
            }
		}
		return instance;
	}

    public bool Process(Core.Net.Client c, Core.Net.Package p, int messageid, SNet.NFunction pf)
    {
        if (c.isConnected)
	    {
		    StringBuilder sb = new StringBuilder();
		    sb.Append(pf.Nm.Name);
		    sb.Append(".");
		    sb.Append(pf.Name);

		    Logger.Log(string.Format("<color=#00ab00ff>>>>receive<<<,msgid:{0}({1})</color>", messageid, sb.ToString()));

		    if (null != this.OnReceive)
		    {
			    this.OnReceive(messageid, p.Header.GetExtData(), p.Header.GetSeqNum());
		    }

		    if (null != this.OnReceivePackage)
		    {
			    this.OnReceivePackage(messageid, p.Data);
		    }

//		    LuaFunction func = XLuaManager.Instance.GetLuaEnv().Global.Get<LuaFunction>(c.DispatchFunc);
		    if (null != this.DispatchFunc)
		    {
			    var luaEnv = XLuaManager.Instance.GetLuaEnv();
			    var L = luaEnv.L;
			    
			    var translator = luaEnv.translator;
			    int oldTop = LuaAPI.lua_gettop(L);
			    
			    int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);                
			    LuaAPI.lua_getref(L, this.DispatchFunc.GetLuaReference());
			    
			    translator.PushAny(L, c.SocketID);
			    translator.PushAny(L, messageid);
			    translator.PushAny(L, p.Header.GetExtData());
			    translator.PushAny(L, p.Header.GetSeqNum());
			    translator.PushAny(L, sb.ToString());
			    int valueCount = 5;


			    IList<NParam> param_list = pf.ParamList;
			    if (p.Data != null)
			    {
				    MemoryStream ms = new MemoryStream(p.Data);
				    for (int i = 0; i < param_list.Count; i++)
				    {
					    push_to_lua(param_list[i], ms, L);
					    valueCount++;
				    }
			    }

			    int error = LuaAPI.lua_pcall(L, valueCount, -1, errFunc);
			    if (error != 0)
				    luaEnv.ThrowExceptionFromError(oldTop);
			    
			    LuaAPI.lua_remove(L, errFunc);
		    }
	    }

	    return true;
    }

	public bool ProcessEvent(Core.Net.Client c, Core.Net.Event.Type type, int error)
	{
		if(type == Core.Net.Event.Type.CONNECT)
		{
			return Connection(c);
		}
		else if(type == Core.Net.Event.Type.CLOSE)
		{
			Logger.LogError("Connect close ------------------------------------");
			return Close(c);
		}
		else if(type == Core.Net.Event.Type.ERROR)
		{
			//Core.Game.GameManager.bDisconnect = true;
			return Error(c, error);
		}
		else
		{
			Logger.LogError("ProcessEvent() failed.. reason: unknown event type");
		}

		return true;
    }

	public bool Connection(Core.Net.Client c)
    {
	    ClientObject obj;
		ClientObject.cache_by_client.TryGetValue(c, out obj);
		this.OnConnecting(obj);
		return true;
    }

	public bool Close(Core.Net.Client c)
    {
	    ClientObject obj;
	    ClientObject.cache_by_client.TryGetValue(c, out obj);
	    this.OnClose(obj);
	    return true;
	}

	public bool Error(Core.Net.Client c, int etype)
    {
	    ClientObject obj;
	    ClientObject.cache_by_client.TryGetValue(c, out obj);
	    this.OnError(obj, etype);
	    return true;
	}

	#region 注册监听事件
	[CSharpCallLua]
	public delegate void FuncOnConnecting(ClientObject clientObject);
	[CSharpCallLua]
	public delegate void FuncOnClose(ClientObject clientObject);
	[CSharpCallLua]
	public delegate void FuncOnError(ClientObject clientObject, int etype);
	[CSharpCallLua]
	public delegate void FuncOnSend(int messageid, UInt32 a, uint b);
	[CSharpCallLua]
	public delegate void FuncOnReceive(int messageid, UInt32 a, uint b);
	[CSharpCallLua]
	public delegate void FuncOnReceivePackage(int messageid, Byte[] t);

	private FuncOnConnecting OnConnecting;
	private FuncOnClose OnClose;
	private FuncOnError OnError;
	private FuncOnSend OnSend;
	private FuncOnReceive OnReceive;
	private FuncOnReceivePackage OnReceivePackage;
	private LuaFunction DispatchFunc;

	public void SetConnectLinster(FuncOnConnecting onConnect, FuncOnClose onClose, FuncOnError onError)
	{
		this.OnConnecting = onConnect;
		this.OnClose = onClose;
		this.OnError = onError;
	}
	
	public void SetOnSend(FuncOnSend onSend)
	{
		this.OnSend = onSend;
	}
	
	public void SetOnReceive(FuncOnReceive onReceive)
	{
		this.OnReceive = onReceive;
	}

	public void SetOnReceivePackage(FuncOnReceivePackage onReceivePackage)
	{
		this.OnReceivePackage = onReceivePackage;
	}
	
	public void SetDispatchFunc(LuaFunction dispatchFunc)
	{
		this.DispatchFunc = dispatchFunc;
	}
	#endregion
	
    private Dictionary<int, string> m_allSendMessage = new Dictionary<int, string>();
	public UInt32 Send(Client c, int messageid)
	{
		ClientObject obj;
		ClientObject.cache_by_client.TryGetValue(c, out obj);
		
		IntPtr L = XLuaManager.Instance.GetLuaEnv().rawL;
		NFunction f = NModuleManager.GetInstance().GetFunctin(messageid);
		if (f != null)
		{
            Package p = c.PackageFactory.CreatePackage();
            p.MessageID = messageid;
            p.Header.SetExtData(c.NextSendPackageExtData);
//            Logger.Log("send message id is " + messageid);
			FormatPackage(p, f, L);
			c.Send(p);
			if (null != this.OnSend)
			{
				this.OnSend(messageid, p.Header.GetExtData(), p.Header.GetSeqNum());
			}

#if UNITY_EDITOR
			StringBuilder sb = new StringBuilder();
			sb.Append(f.Nm.Name);
			sb.Append(".");
			sb.Append(f.Name);
            if(!m_allSendMessage.ContainsKey(messageid))
            {
                m_allSendMessage.Add(messageid, sb.ToString());
                if(!m_allHandleMessage.ContainsKey(sb.ToString()))
                {
//                    Logger.Log(string.Format("<color=#ff9900ff><<<need handle message>>>,id:{0} name:{1}</color>", messageid, sb.ToString()));
                    m_allHandleMessage.Add(sb.ToString(), 1);
                }
                
            }
            //Logger.Log(string.Format("<color=#00ababff><<<send>>>,msgid:{0}({1})</color>", messageid, sb.ToString()), Logger.LogLevel.Important); 
#endif
            return p.Header.GetExtData();
		}
        return 0;
	}

	#region lua数据打包
	private void FormatPackage(Package p, NFunction f, RealStatePtr L)
    {
        int startparamnum = 3;
        IList<NParam> parmlist = f.ParamList;
        using (MemoryStream ms = new MemoryStream())
        {
            for (int i = 0; i < parmlist.Count; i++)
            {
                Sio.SData d = format_data(parmlist[i], L, ++startparamnum, true);
                if (d != null)
                {
                    d.Serializ(ms);
                }
            }
            byte[] dbyte = new byte[ms.Length];
            Array.Copy(ms.GetBuffer(), dbyte, ms.Length);
            p.Data = dbyte;
        }
    }
    
    private Sio.SData format_data(NParam o, RealStatePtr L, int index, bool checklist)
		{
			if (checklist == true && o.Container == ParamContainer.pparam_container_list)
			{
				return format_List(L, index, o);
			}
			else
			{
				switch (o.DType)
				{
					case ParamType.ptype_bool:
						{
							bool ans = LuaAPI.lua_toboolean(L, index);
							if (LuaAPI.lua_isboolean(L, index))
							{
								ans = LuaAPI.lua_toboolean(L, index);
							}
							return new Sio.SData(ans);
						}
					case ParamType.ptype_char:
					case ParamType.ptype_uchar:
						{
							byte ans = 0;
							if (LuaAPI.lua_isnumber(L, index))
							{
								ans = (byte)LuaAPI.xlua_tointeger(L, index);
							}
							return new Sio.SData(ans);
						}
					case ParamType.ptype_ushort:
						{
							UInt16 ans = 0;
							if (LuaAPI.lua_isnumber(L, index))
							{
								ans = (UInt16)LuaAPI.xlua_tointeger(L, index);
							}
							return new Sio.SData(ans);
						}
					case ParamType.ptype_short:
						{
							Int16 ans = 0;
							if (LuaAPI.lua_isnumber(L, index))
							{
								ans = (Int16)LuaAPI.xlua_tointeger(L, index);
							}
							return new Sio.SData(ans);
						}
					case ParamType.ptype_int:
						{
							int ans = 0;
							if (LuaAPI.lua_isnumber(L, index))
							{
								ans = (int)LuaAPI.xlua_tointeger(L, index);
							}
							return new Sio.SData(ans);
						}
					case ParamType.ptype_uint:
						{
							uint ans = 0;
							if (LuaAPI.lua_isnumber(L, index))
							{
								ans = (uint)LuaAPI.xlua_tointeger(L, index);
							}
							return new Sio.SData(ans);
						}
					case ParamType.ptype_float:
						{
							float ans = 0.0f;
							if (LuaAPI.lua_isnumber(L, index))
							{
								ans = (float)LuaAPI.xlua_tointeger(L, index);
							}
							return new Sio.SData(ans);
						}
					case ParamType.ptype_double:
						{
							double ans = 0.0;
							if (LuaAPI.lua_isnumber(L, index))
							{
								ans = (double)LuaAPI.xlua_tointeger(L, index);
							}
							return new Sio.SData(ans);
						}
					case ParamType.ptype_long:
						{
							long ans = 0;
							if (LuaAPI.lua_isint64(L, index))
							{
								ans = LuaAPI.lua_toint64(L, index);
							}
							return new Sio.SData(ans);
						}
					case ParamType.ptype_ulong:
						{
							ulong ans = 0;
	                        if (LuaAPI.lua_isint64(L, index))
	                        {
	                            ans = LuaAPI.lua_touint64(L, index);
	                        }
	                        return new Sio.SData(ans);
						}
					case ParamType.ptype_string:
						{
							string ans = string.Empty;
							if (LuaAPI.lua_isstring(L, index))
							{
								ans = LuaAPI.lua_tostring(L, index);
							}
							return new Sio.SData(ans);
						}
					case ParamType.ptype_object:
						{
							return format_map(L, index, o);
						}
					default:
						break;
				}
			}
			return null;
		}
    
    
    private Sio.SData format_map(IntPtr L, int index, NParam o)
    {
	    Sio.SMapWriter pmap = new Sio.SMapWriter();
	    Sio.SData data = new Sio.SData(pmap);
	    
	    if (LuaAPI.lua_istable(L, index) && o.DType == ParamType.ptype_object)
	    {
		    NStruct ps = NStructManager.GetInstance().Find(o.TypeName);
		    if (ps != null)
		    {
			    LuaAPI.lua_pushnil(L);
			    while (LuaAPI.lua_next(L, index) != 0)
			    {
				    if (LuaAPI.lua_isstring( L,-2))
				    {

					    NParam findparam = ps.Get(LuaAPI.lua_tostring(L,-2));
					    if (findparam!=null)
					    {
						    pmap.write(findparam.Id, format_data(findparam, L, LuaAPI.lua_gettop(L), true));
					    }
				    }
				    LuaAPI.lua_pop(L, 1);
			    }
		    }
	    }
	    return data;
    }
    
    private Sio.SData format_List(IntPtr L, int index, NParam o)
    {
	    Sio.SListWriter pl = new Sio.SListWriter();
	    Sio.SData d = new Sio.SData(pl);
	    if (LuaAPI.lua_istable(L, index))
	    {
		    int indexsize = (int)LuaAPI.xlua_objlen(L, index);
		    for (int list_index = 1; list_index <= indexsize; ++list_index)
		    {
			    LuaAPI.xlua_rawgeti(L, index, list_index);
			    Sio.SData td = format_data(o, L, LuaAPI.lua_gettop(L), false);
			    pl.add(td);
			    LuaAPI.lua_pop(L,1);
		    }
	    }
	    return d;
    }
    #endregion

    #region 数据转换lua

	private void push_to_lua(NParam o, MemoryStream ms, IntPtr L)
	{ 
		Sio.SDataBuff data = new Sio.SDataBuff();
		data.UnSerializ(ms);
		push_data(data, L, o);
	}

	private void push_data(Sio.SDataBuff data, IntPtr L, NParam o)
	{
		switch (data.type)
		{
			case SType.stype_bool:
				LuaAPI.lua_pushboolean(L, data.boolValue);
				break;
			case SType.stype_uchar:
				LuaAPI.xlua_pushuint(L, data.ucharValue);
				break;
			case SType.stype_char:
				LuaAPI.xlua_pushinteger(L, data.charValue);
				break;
			case SType.stype_short:
				LuaAPI.xlua_pushinteger(L, data.shortValue);
				break;
			case SType.stype_ushort:
				LuaAPI.xlua_pushuint(L, data.ushortValue);
				break;
			case SType.stype_int:
				LuaAPI.xlua_pushinteger(L, data.intValue);
				break;
			case SType.stype_uint:
				LuaAPI.xlua_pushuint(L, data.uintValue);
				break;
			case SType.stype_float:
				LuaAPI.lua_pushnumber(L, data.floatValue);
				break;
			case SType.stype_double:
				LuaAPI.lua_pushnumber(L, data.doubleValue);
				break;
			case SType.stype_long:
				LuaAPI.lua_pushint64(L, data.longValue);
				break;
			case SType.stype_string:
				LuaAPI.lua_pushstring(L, data.stringValue);
				break;
			case SType.stype_list:
				push_list(data, o, L);
				break;
			case SType.stype_map:
				push_map(data, o, L);
				break;
			default:
				Logger.LogError("push_data error!!!!!!!!!!!!!!! type:" + data.type.ToString());
				break;
		}
	}

	private void push_map(SDataBuff data, NParam o, IntPtr L)
	{

		SMapReader pmap = data.mapReader;			
		LuaAPI.lua_newtable(L);
		if (pmap != null && o.DType == ParamType.ptype_object)
		{
			NStruct ps = NStructManager.GetInstance().Find(o.TypeName);
			if (ps != null)
			{
				SDataBuff k = new SDataBuff();
				SDataBuff v = new SDataBuff();
				while (pmap.Next(k, v))
				{
					NParam p = ps.Get(k.intValue);
					if (p!=null)
					{
						LuaAPI.lua_pushstring(L, p.Name);
						push_data(v, L, p);
						LuaAPI.xlua_psettable(L, -3);
					}
				}
			}
		}
	}
	private void push_list(SDataBuff data, NParam o, IntPtr L)
	{
		Sio.SListReader plist = data.listReader;
		LuaAPI.lua_newtable(L);
		int top_index = LuaAPI.lua_gettop(L);
		int luaindex = 0;
		if (plist != null && o.Container == ParamContainer.pparam_container_list)
		{
			Sio.SDataBuff d = new Sio.SDataBuff();
			while (plist.Next(d))
			{
				push_data(d, L, o);
				LuaAPI.xlua_rawseti(L, top_index, ++luaindex);
			}
		}
	}
    #endregion
}
