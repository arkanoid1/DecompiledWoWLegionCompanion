using bgs.types;
using bnet.protocol.attribute;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace bgs
{
	public static class BnetParty
	{
		public enum FriendlyGameRoleSet
		{
			Inviter = 1,
			Invitee = 2
		}

		public enum SpectatorPartyRoleSet
		{
			Member = 1,
			Leader = 2
		}

		public delegate void PartyErrorHandler(PartyError error);

		public delegate void JoinedHandler(OnlineEventType evt, PartyInfo party, LeaveReason? reason);

		public delegate void PrivacyLevelChangedHandler(PartyInfo party, PrivacyLevel newPrivacyLevel);

		public delegate void MemberEventHandler(OnlineEventType evt, PartyInfo party, BnetGameAccountId memberId, bool isRolesUpdate, LeaveReason? reason);

		public delegate void ReceivedInviteHandler(OnlineEventType evt, PartyInfo party, ulong inviteId, InviteRemoveReason? reason);

		public delegate void SentInviteHandler(OnlineEventType evt, PartyInfo party, ulong inviteId, bool senderIsMyself, InviteRemoveReason? reason);

		public delegate void ReceivedInviteRequestHandler(OnlineEventType evt, PartyInfo party, InviteRequest request, InviteRequestRemovedReason? reason);

		public delegate void ChatMessageHandler(PartyInfo party, BnetGameAccountId speakerId, string chatMessage);

		public delegate void PartyAttributeChangedHandler(PartyInfo party, string attributeKey, Variant attributeValue);

		public delegate void CreateSuccessCallback(PartyType type, PartyId newlyCreatedPartyId);

		public const string ATTRIBUTE_PARTY_TYPE = "WTCG.Party.Type";

		public const string ATTRIBUTE_PARTY_CREATOR = "WTCG.Party.Creator";

		public const string ATTRIBUTE_SCENARIO_ID = "WTCG.Game.ScenarioId";

		public const string ATTRIBUTE_FRIENDLY_DECLINE_REASON = "WTCG.Friendly.DeclineReason";

		public const string ATTRIBUTE_PARTY_SERVER_INFO = "WTCG.Party.ServerInfo";

		private static Map<BnetFeatureEvent, HashSet<BattleNetErrors>> s_ignorableErrorCodes;

		private static Map<PartyId, PartyType> s_joinedParties;

		private static Map<PartyType, BnetParty.CreateSuccessCallback> s_pendingPartyCreates;

		private static Map<string, List<BnetParty.PartyAttributeChangedHandler>> s_attributeChangedSubscribers;

		public static event BnetParty.PartyErrorHandler OnError;

		public static event BnetParty.JoinedHandler OnJoined;

		public static event BnetParty.PrivacyLevelChangedHandler OnPrivacyLevelChanged;

		public static event BnetParty.MemberEventHandler OnMemberEvent;

		public static event BnetParty.ReceivedInviteHandler OnReceivedInvite;

		public static event BnetParty.SentInviteHandler OnSentInvite;

		public static event BnetParty.ReceivedInviteRequestHandler OnReceivedInviteRequest;

		public static event BnetParty.ChatMessageHandler OnChatMessage;

		public static event BnetParty.PartyAttributeChangedHandler OnPartyAttributeChanged;

		static BnetParty()
		{
			// Note: this type is marked as 'beforefieldinit'.
			Map<BnetFeatureEvent, HashSet<BattleNetErrors>> map = new Map<BnetFeatureEvent, HashSet<BattleNetErrors>>();
			Map<BnetFeatureEvent, HashSet<BattleNetErrors>> arg_1C_0 = map;
			BnetFeatureEvent arg_1C_1 = BnetFeatureEvent.Party_KickMember_Callback;
			HashSet<BattleNetErrors> hashSet = new HashSet<BattleNetErrors>();
			hashSet.Add(BattleNetErrors.ERROR_CHANNEL_NO_SUCH_MEMBER);
			arg_1C_0.Add(arg_1C_1, hashSet);
			Map<BnetFeatureEvent, HashSet<BattleNetErrors>> arg_43_0 = map;
			BnetFeatureEvent arg_43_1 = BnetFeatureEvent.Party_Leave_Callback;
			hashSet = new HashSet<BattleNetErrors>();
			hashSet.Add(BattleNetErrors.ERROR_CHANNEL_NOT_MEMBER);
			hashSet.Add(BattleNetErrors.ERROR_CHANNEL_NO_CHANNEL);
			arg_43_0.Add(arg_43_1, hashSet);
			BnetParty.s_ignorableErrorCodes = map;
			BnetParty.s_joinedParties = new Map<PartyId, PartyType>();
			BnetParty.s_pendingPartyCreates = null;
			BnetParty.s_attributeChangedSubscribers = null;
		}

		public static void RegisterAttributeChangedHandler(string attributeKey, BnetParty.PartyAttributeChangedHandler handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			if (BnetParty.s_attributeChangedSubscribers == null)
			{
				BnetParty.s_attributeChangedSubscribers = new Map<string, List<BnetParty.PartyAttributeChangedHandler>>();
			}
			List<BnetParty.PartyAttributeChangedHandler> list;
			if (!BnetParty.s_attributeChangedSubscribers.TryGetValue(attributeKey, out list))
			{
				list = new List<BnetParty.PartyAttributeChangedHandler>();
				BnetParty.s_attributeChangedSubscribers[attributeKey] = list;
			}
			if (!list.Contains(handler))
			{
				list.Add(handler);
			}
		}

		public static bool UnregisterAttributeChangedHandler(string attributeKey, BnetParty.PartyAttributeChangedHandler handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			if (BnetParty.s_attributeChangedSubscribers == null)
			{
				return false;
			}
			List<BnetParty.PartyAttributeChangedHandler> list = null;
			return BnetParty.s_attributeChangedSubscribers.TryGetValue(attributeKey, out list) && list.Remove(handler);
		}

		public static bool IsInParty(PartyId partyId)
		{
			return !(partyId == null) && BnetParty.s_joinedParties.ContainsKey(partyId);
		}

		public static PartyId[] GetJoinedPartyIds()
		{
			return Enumerable.ToArray<PartyId>(BnetParty.s_joinedParties.Keys);
		}

		public static PartyInfo[] GetJoinedParties()
		{
			return Enumerable.ToArray<PartyInfo>(Enumerable.Select<KeyValuePair<PartyId, PartyType>, PartyInfo>(BnetParty.s_joinedParties, (KeyValuePair<PartyId, PartyType> kv) => new PartyInfo(kv.get_Key(), kv.get_Value())));
		}

		public static PartyInfo GetJoinedParty(PartyId partyId)
		{
			if (partyId == null)
			{
				return null;
			}
			PartyType type = PartyType.DEFAULT;
			if (BnetParty.s_joinedParties.TryGetValue(partyId, out type))
			{
				return new PartyInfo(partyId, type);
			}
			return null;
		}

		public static PartyType GetPartyType(PartyId partyId)
		{
			PartyType result = PartyType.DEFAULT;
			if (partyId == null)
			{
				return result;
			}
			BnetParty.s_joinedParties.TryGetValue(partyId, out result);
			return result;
		}

		public static bool IsLeader(PartyId partyId)
		{
			if (partyId == null)
			{
				return false;
			}
			PartyMember myselfMember = BnetParty.GetMyselfMember(partyId);
			if (myselfMember != null)
			{
				PartyType partyType = BnetParty.GetPartyType(partyId);
				if (myselfMember.IsLeader(partyType))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsMember(PartyId partyId, BnetGameAccountId memberId)
		{
			if (partyId == null)
			{
				return false;
			}
			PartyMember member = BnetParty.GetMember(partyId, memberId);
			return member != null;
		}

		public static bool IsPartyFull(PartyId partyId, bool includeInvites = true)
		{
			if (partyId == null)
			{
				return false;
			}
			int num = BnetParty.CountMembers(partyId);
			int num2 = (!includeInvites) ? 0 : BnetParty.GetSentInvites(partyId).Length;
			int maxPartyMembers = BattleNet.GetMaxPartyMembers(partyId.ToEntityId());
			return num + num2 >= maxPartyMembers;
		}

		public static int CountMembers(PartyId partyId)
		{
			if (partyId == null)
			{
				return 0;
			}
			return BattleNet.GetCountPartyMembers(partyId.ToEntityId());
		}

		public static PrivacyLevel GetPrivacyLevel(PartyId partyId)
		{
			if (partyId == null)
			{
				return PrivacyLevel.CLOSED;
			}
			return (PrivacyLevel)BattleNet.GetPartyPrivacy(partyId.ToEntityId());
		}

		public static PartyMember GetMember(PartyId partyId, BnetGameAccountId memberId)
		{
			PartyMember[] members = BnetParty.GetMembers(partyId);
			for (int i = 0; i < members.Length; i++)
			{
				PartyMember partyMember = members[i];
				if (partyMember.GameAccountId == memberId)
				{
					return partyMember;
				}
			}
			return null;
		}

		public static PartyMember GetLeader(PartyId partyId)
		{
			if (partyId == null)
			{
				return null;
			}
			PartyMember[] members = BnetParty.GetMembers(partyId);
			PartyType partyType = BnetParty.GetPartyType(partyId);
			for (int i = 0; i < members.Length; i++)
			{
				PartyMember partyMember = members[i];
				if (partyMember.IsLeader(partyType))
				{
					return partyMember;
				}
			}
			return null;
		}

		public static PartyMember GetMyselfMember(PartyId partyId)
		{
			if (partyId == null)
			{
				return null;
			}
			BnetGameAccountId bnetGameAccountId = BnetGameAccountId.CreateFromEntityId(BattleNet.GetMyGameAccountId());
			if (bnetGameAccountId == null)
			{
				return null;
			}
			return BnetParty.GetMember(partyId, bnetGameAccountId);
		}

		public static PartyMember[] GetMembers(PartyId partyId)
		{
			if (partyId == null)
			{
				return new PartyMember[0];
			}
			bgs.types.PartyMember[] array;
			BattleNet.GetPartyMembers(partyId.ToEntityId(), out array);
			PartyMember[] array2 = new PartyMember[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				bgs.types.PartyMember partyMember = array[i];
				array2[i] = new PartyMember
				{
					GameAccountId = BnetGameAccountId.CreateFromEntityId(partyMember.memberGameAccountId),
					RoleIds = new uint[]
					{
						partyMember.firstMemberRole
					}
				};
			}
			return array2;
		}

		public static PartyInvite GetReceivedInvite(ulong inviteId)
		{
			PartyInvite[] receivedInvites = BnetParty.GetReceivedInvites();
			return Enumerable.FirstOrDefault<PartyInvite>(receivedInvites, (PartyInvite i) => i.InviteId == inviteId);
		}

		public static PartyInvite GetReceivedInviteFrom(BnetGameAccountId inviterId, PartyType partyType)
		{
			PartyInvite[] receivedInvites = BnetParty.GetReceivedInvites();
			return Enumerable.FirstOrDefault<PartyInvite>(receivedInvites, (PartyInvite i) => i.InviterId == inviterId && i.PartyType == partyType);
		}

		public static PartyInvite[] GetReceivedInvites()
		{
			PartyInvite[] result;
			BattleNet.GetReceivedPartyInvites(out result);
			return result;
		}

		public static PartyInvite GetSentInvite(PartyId partyId, ulong inviteId)
		{
			if (partyId == null)
			{
				return null;
			}
			PartyInvite[] sentInvites = BnetParty.GetSentInvites(partyId);
			return Enumerable.FirstOrDefault<PartyInvite>(sentInvites, (PartyInvite i) => i.InviteId == inviteId);
		}

		public static PartyInvite[] GetSentInvites(PartyId partyId)
		{
			if (partyId == null)
			{
				return new PartyInvite[0];
			}
			PartyInvite[] result;
			BattleNet.GetPartySentInvites(partyId.ToEntityId(), out result);
			return result;
		}

		public static InviteRequest[] GetInviteRequests(PartyId partyId)
		{
			if (partyId == null)
			{
				return new InviteRequest[0];
			}
			InviteRequest[] result;
			BattleNet.GetPartyInviteRequests(partyId.ToEntityId(), out result);
			return result;
		}

		public static KeyValuePair<string, object>[] GetAllPartyAttributes(PartyId partyId)
		{
			if (partyId == null)
			{
				return new KeyValuePair<string, object>[0];
			}
			string[] array;
			BattleNet.GetAllPartyAttributes(partyId.ToEntityId(), out array);
			KeyValuePair<string, object>[] array2 = new KeyValuePair<string, object>[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				string text = array[i];
				object obj = null;
				long? partyAttributeLong = BnetParty.GetPartyAttributeLong(partyId, text);
				if (partyAttributeLong.get_HasValue())
				{
					obj = partyAttributeLong;
				}
				string partyAttributeString = BnetParty.GetPartyAttributeString(partyId, text);
				if (partyAttributeString != null)
				{
					obj = partyAttributeString;
				}
				byte[] partyAttributeBlob = BnetParty.GetPartyAttributeBlob(partyId, text);
				if (partyAttributeBlob != null)
				{
					obj = partyAttributeBlob;
				}
				array2[i] = new KeyValuePair<string, object>(text, obj);
			}
			return array2;
		}

		public static Variant GetPartyAttributeVariant(PartyId partyId, string attributeKey)
		{
			Variant variant = new Variant();
			EntityId partyId2 = partyId.ToEntityId();
			long intValue;
			if (BattleNet.GetPartyAttributeLong(partyId2, attributeKey, out intValue))
			{
				variant.IntValue = intValue;
			}
			else
			{
				string text;
				BattleNet.GetPartyAttributeString(partyId2, attributeKey, out text);
				if (text != null)
				{
					variant.StringValue = text;
				}
				else
				{
					byte[] array;
					BattleNet.GetPartyAttributeBlob(partyId2, attributeKey, out array);
					if (array != null)
					{
						variant.BlobValue = array;
					}
				}
			}
			return variant;
		}

		public static long? GetPartyAttributeLong(PartyId partyId, string attributeKey)
		{
			if (partyId == null)
			{
				return default(long?);
			}
			long num;
			if (BattleNet.GetPartyAttributeLong(partyId.ToEntityId(), attributeKey, out num))
			{
				return new long?(num);
			}
			return default(long?);
		}

		public static string GetPartyAttributeString(PartyId partyId, string attributeKey)
		{
			if (partyId == null)
			{
				return null;
			}
			string result;
			BattleNet.GetPartyAttributeString(partyId.ToEntityId(), attributeKey, out result);
			return result;
		}

		public static byte[] GetPartyAttributeBlob(PartyId partyId, string attributeKey)
		{
			if (partyId == null)
			{
				return null;
			}
			byte[] result;
			BattleNet.GetPartyAttributeBlob(partyId.ToEntityId(), attributeKey, out result);
			return result;
		}

		public static void CreateParty(PartyType partyType, PrivacyLevel privacyLevel, byte[] creatorBlob, BnetParty.CreateSuccessCallback successCallback)
		{
			string @string = EnumUtils.GetString<PartyType>(partyType);
			if (BnetParty.s_pendingPartyCreates != null && BnetParty.s_pendingPartyCreates.ContainsKey(partyType))
			{
				BnetParty.RaisePartyError(true, @string, BnetFeatureEvent.Party_Create_Callback, "CreateParty: Already creating party of type {0}", new object[]
				{
					partyType
				});
				return;
			}
			if (BnetParty.s_pendingPartyCreates == null)
			{
				BnetParty.s_pendingPartyCreates = new Map<PartyType, BnetParty.CreateSuccessCallback>();
			}
			BnetParty.s_pendingPartyCreates[partyType] = successCallback;
			BattleNet.CreateParty(@string, (int)privacyLevel, creatorBlob);
		}

		public static void JoinParty(PartyId partyId, PartyType partyType)
		{
			EntityId partyId2 = partyId.ToEntityId();
			BattleNet.JoinParty(partyId2, EnumUtils.GetString<PartyType>(partyType));
		}

		public static void Leave(PartyId partyId)
		{
			if (!BnetParty.IsInParty(partyId))
			{
				return;
			}
			EntityId partyId2 = partyId.ToEntityId();
			BattleNet.LeaveParty(partyId2);
		}

		public static void DissolveParty(PartyId partyId)
		{
			if (!BnetParty.IsInParty(partyId))
			{
				return;
			}
			EntityId partyId2 = partyId.ToEntityId();
			BattleNet.DissolveParty(partyId2);
		}

		public static void SetPrivacy(PartyId partyId, PrivacyLevel privacyLevel)
		{
			if (!BnetParty.IsInParty(partyId))
			{
				return;
			}
			EntityId partyId2 = partyId.ToEntityId();
			BattleNet.SetPartyPrivacy(partyId2, (int)privacyLevel);
		}

		public static void SetLeader(PartyId partyId, BnetGameAccountId memberId)
		{
			if (!BnetParty.IsInParty(partyId))
			{
				return;
			}
			EntityId partyId2 = partyId.ToEntityId();
			EntityId memberId2 = BnetEntityId.CreateEntityId(memberId);
			PartyType partyType = BnetParty.GetPartyType(partyId);
			uint leaderRoleId = PartyMember.GetLeaderRoleId(partyType);
			BattleNet.AssignPartyRole(partyId2, memberId2, leaderRoleId);
		}

		public static void SendInvite(PartyId toWhichPartyId, BnetGameAccountId recipientId)
		{
			if (!BnetParty.IsInParty(toWhichPartyId))
			{
				return;
			}
			EntityId partyId = toWhichPartyId.ToEntityId();
			EntityId inviteeId = BnetEntityId.CreateEntityId(recipientId);
			BattleNet.SendPartyInvite(partyId, inviteeId, false);
		}

		public static void AcceptReceivedInvite(ulong inviteId)
		{
			BattleNet.AcceptPartyInvite(inviteId);
		}

		public static void DeclineReceivedInvite(ulong inviteId)
		{
			BattleNet.DeclinePartyInvite(inviteId);
		}

		public static void RevokeSentInvite(PartyId partyId, ulong inviteId)
		{
			if (!BnetParty.IsInParty(partyId))
			{
				return;
			}
			BattleNet.RevokePartyInvite(partyId.ToEntityId(), inviteId);
		}

		public static void RequestInvite(PartyId partyId, BnetGameAccountId whomToAskForApproval, BnetGameAccountId whomToInvite, PartyType partyType)
		{
			if (BnetParty.IsLeader(partyId))
			{
				PartyError error = default(PartyError);
				error.IsOperationCallback = true;
				error.DebugContext = "RequestInvite";
				error.ErrorCode = BattleNetErrors.ERROR_INVALID_TARGET_ID;
				error.Feature = BnetFeature.Party;
				error.FeatureEvent = BnetFeatureEvent.Party_RequestPartyInvite_Callback;
				error.PartyId = partyId;
				error.szPartyType = EnumUtils.GetString<PartyType>(partyType);
				error.StringData = "leaders cannot RequestInvite - use SendInvite instead.";
				BnetParty.OnError(error);
				return;
			}
			EntityId partyId2 = partyId.ToEntityId();
			EntityId whomToAskForApproval2 = BnetEntityId.CreateEntityId(whomToAskForApproval);
			EntityId whomToInvite2 = BnetEntityId.CreateEntityId(whomToInvite);
			string @string = EnumUtils.GetString<PartyType>(partyType);
			BattleNet.RequestPartyInvite(partyId2, whomToAskForApproval2, whomToInvite2, @string);
		}

		public static void AcceptInviteRequest(PartyId partyId, BnetGameAccountId requestedTargetId)
		{
			BnetParty.SendInvite(partyId, requestedTargetId);
		}

		public static void IgnoreInviteRequest(PartyId partyId, BnetGameAccountId requestedTargetId)
		{
			EntityId partyId2 = partyId.ToEntityId();
			EntityId requestedTargetId2 = BnetEntityId.CreateEntityId(requestedTargetId);
			BattleNet.IgnoreInviteRequest(partyId2, requestedTargetId2);
		}

		public static void KickMember(PartyId partyId, BnetGameAccountId memberId)
		{
			if (!BnetParty.IsInParty(partyId))
			{
				return;
			}
			EntityId partyId2 = partyId.ToEntityId();
			EntityId memberId2 = BnetEntityId.CreateEntityId(memberId);
			BattleNet.KickPartyMember(partyId2, memberId2);
		}

		public static void SendChatMessage(PartyId partyId, string chatMessage)
		{
			if (!BnetParty.IsInParty(partyId))
			{
				return;
			}
			BattleNet.SendPartyChatMessage(partyId.ToEntityId(), chatMessage);
		}

		public static void ClearPartyAttribute(PartyId partyId, string attributeKey)
		{
			BattleNet.ClearPartyAttribute(partyId.ToEntityId(), attributeKey);
		}

		public static void SetPartyAttributeLong(PartyId partyId, string attributeKey, long value)
		{
			BattleNet.SetPartyAttributeLong(partyId.ToEntityId(), attributeKey, value);
		}

		public static void SetPartyAttributeString(PartyId partyId, string attributeKey, string value)
		{
			BattleNet.SetPartyAttributeString(partyId.ToEntityId(), attributeKey, value);
		}

		public static void SetPartyAttributeBlob(PartyId partyId, string attributeKey, byte[] value)
		{
			BattleNet.SetPartyAttributeBlob(partyId.ToEntityId(), attributeKey, value);
		}

		public static void RemoveFromAllEventHandlers(object targetObject)
		{
			Type type = (targetObject != null) ? targetObject.GetType() : null;
			if (BnetParty.OnError != null)
			{
				IEnumerator enumerator = (BnetParty.OnError.GetInvocationList().Clone() as Array).GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Delegate @delegate = (Delegate)enumerator.get_Current();
						if (@delegate.get_Target() == targetObject || (@delegate.get_Target() == null && @delegate.get_Method().get_DeclaringType() == type))
						{
							BnetParty.OnError = (BnetParty.PartyErrorHandler)Delegate.Remove(BnetParty.OnError, (BnetParty.PartyErrorHandler)@delegate);
						}
					}
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
			if (BnetParty.OnJoined != null)
			{
				IEnumerator enumerator2 = (BnetParty.OnJoined.GetInvocationList().Clone() as Array).GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						Delegate delegate2 = (Delegate)enumerator2.get_Current();
						if (delegate2.get_Target() == targetObject || (delegate2.get_Target() == null && delegate2.get_Method().get_DeclaringType() == type))
						{
							BnetParty.OnJoined = (BnetParty.JoinedHandler)Delegate.Remove(BnetParty.OnJoined, (BnetParty.JoinedHandler)delegate2);
						}
					}
				}
				finally
				{
					IDisposable disposable2 = enumerator2 as IDisposable;
					if (disposable2 != null)
					{
						disposable2.Dispose();
					}
				}
			}
			if (BnetParty.OnPrivacyLevelChanged != null)
			{
				IEnumerator enumerator3 = (BnetParty.OnPrivacyLevelChanged.GetInvocationList().Clone() as Array).GetEnumerator();
				try
				{
					while (enumerator3.MoveNext())
					{
						Delegate delegate3 = (Delegate)enumerator3.get_Current();
						if (delegate3.get_Target() == targetObject || (delegate3.get_Target() == null && delegate3.get_Method().get_DeclaringType() == type))
						{
							BnetParty.OnPrivacyLevelChanged = (BnetParty.PrivacyLevelChangedHandler)Delegate.Remove(BnetParty.OnPrivacyLevelChanged, (BnetParty.PrivacyLevelChangedHandler)delegate3);
						}
					}
				}
				finally
				{
					IDisposable disposable3 = enumerator3 as IDisposable;
					if (disposable3 != null)
					{
						disposable3.Dispose();
					}
				}
			}
			if (BnetParty.OnMemberEvent != null)
			{
				IEnumerator enumerator4 = (BnetParty.OnMemberEvent.GetInvocationList().Clone() as Array).GetEnumerator();
				try
				{
					while (enumerator4.MoveNext())
					{
						Delegate delegate4 = (Delegate)enumerator4.get_Current();
						if (delegate4.get_Target() == targetObject || (delegate4.get_Target() == null && delegate4.get_Method().get_DeclaringType() == type))
						{
							BnetParty.OnMemberEvent = (BnetParty.MemberEventHandler)Delegate.Remove(BnetParty.OnMemberEvent, (BnetParty.MemberEventHandler)delegate4);
						}
					}
				}
				finally
				{
					IDisposable disposable4 = enumerator4 as IDisposable;
					if (disposable4 != null)
					{
						disposable4.Dispose();
					}
				}
			}
			if (BnetParty.OnReceivedInvite != null)
			{
				IEnumerator enumerator5 = (BnetParty.OnReceivedInvite.GetInvocationList().Clone() as Array).GetEnumerator();
				try
				{
					while (enumerator5.MoveNext())
					{
						Delegate delegate5 = (Delegate)enumerator5.get_Current();
						if (delegate5.get_Target() == targetObject || (delegate5.get_Target() == null && delegate5.get_Method().get_DeclaringType() == type))
						{
							BnetParty.OnReceivedInvite = (BnetParty.ReceivedInviteHandler)Delegate.Remove(BnetParty.OnReceivedInvite, (BnetParty.ReceivedInviteHandler)delegate5);
						}
					}
				}
				finally
				{
					IDisposable disposable5 = enumerator5 as IDisposable;
					if (disposable5 != null)
					{
						disposable5.Dispose();
					}
				}
			}
			if (BnetParty.OnSentInvite != null)
			{
				IEnumerator enumerator6 = (BnetParty.OnSentInvite.GetInvocationList().Clone() as Array).GetEnumerator();
				try
				{
					while (enumerator6.MoveNext())
					{
						Delegate delegate6 = (Delegate)enumerator6.get_Current();
						if (delegate6.get_Target() == targetObject || (delegate6.get_Target() == null && delegate6.get_Method().get_DeclaringType() == type))
						{
							BnetParty.OnSentInvite = (BnetParty.SentInviteHandler)Delegate.Remove(BnetParty.OnSentInvite, (BnetParty.SentInviteHandler)delegate6);
						}
					}
				}
				finally
				{
					IDisposable disposable6 = enumerator6 as IDisposable;
					if (disposable6 != null)
					{
						disposable6.Dispose();
					}
				}
			}
			if (BnetParty.OnReceivedInviteRequest != null)
			{
				IEnumerator enumerator7 = (BnetParty.OnReceivedInviteRequest.GetInvocationList().Clone() as Array).GetEnumerator();
				try
				{
					while (enumerator7.MoveNext())
					{
						Delegate delegate7 = (Delegate)enumerator7.get_Current();
						if (delegate7.get_Target() == targetObject || (delegate7.get_Target() == null && delegate7.get_Method().get_DeclaringType() == type))
						{
							BnetParty.OnReceivedInviteRequest = (BnetParty.ReceivedInviteRequestHandler)Delegate.Remove(BnetParty.OnReceivedInviteRequest, (BnetParty.ReceivedInviteRequestHandler)delegate7);
						}
					}
				}
				finally
				{
					IDisposable disposable7 = enumerator7 as IDisposable;
					if (disposable7 != null)
					{
						disposable7.Dispose();
					}
				}
			}
			if (BnetParty.OnChatMessage != null)
			{
				IEnumerator enumerator8 = (BnetParty.OnChatMessage.GetInvocationList().Clone() as Array).GetEnumerator();
				try
				{
					while (enumerator8.MoveNext())
					{
						Delegate delegate8 = (Delegate)enumerator8.get_Current();
						if (delegate8.get_Target() == targetObject || (delegate8.get_Target() == null && delegate8.get_Method().get_DeclaringType() == type))
						{
							BnetParty.OnChatMessage = (BnetParty.ChatMessageHandler)Delegate.Remove(BnetParty.OnChatMessage, (BnetParty.ChatMessageHandler)delegate8);
						}
					}
				}
				finally
				{
					IDisposable disposable8 = enumerator8 as IDisposable;
					if (disposable8 != null)
					{
						disposable8.Dispose();
					}
				}
			}
			if (BnetParty.OnPartyAttributeChanged != null)
			{
				IEnumerator enumerator9 = (BnetParty.OnPartyAttributeChanged.GetInvocationList().Clone() as Array).GetEnumerator();
				try
				{
					while (enumerator9.MoveNext())
					{
						Delegate delegate9 = (Delegate)enumerator9.get_Current();
						if (delegate9.get_Target() == targetObject || (delegate9.get_Target() == null && delegate9.get_Method().get_DeclaringType() == type))
						{
							BnetParty.OnPartyAttributeChanged = (BnetParty.PartyAttributeChangedHandler)Delegate.Remove(BnetParty.OnPartyAttributeChanged, (BnetParty.PartyAttributeChangedHandler)delegate9);
						}
					}
				}
				finally
				{
					IDisposable disposable9 = enumerator9 as IDisposable;
					if (disposable9 != null)
					{
						disposable9.Dispose();
					}
				}
			}
			if (BnetParty.s_attributeChangedSubscribers != null)
			{
				foreach (KeyValuePair<string, List<BnetParty.PartyAttributeChangedHandler>> current in BnetParty.s_attributeChangedSubscribers)
				{
					var array = Enumerable.ToArray(Enumerable.Select(current.get_Value(), (BnetParty.PartyAttributeChangedHandler h, int idx) => new
					{
						Handler = h,
						Index = idx
					}));
					for (int i = 0; i < array.Length; i++)
					{
						var <>__AnonType = array[i];
						if (<>__AnonType.Handler.get_Target() == targetObject || <>__AnonType.Handler.get_Method().get_DeclaringType() == type)
						{
							current.get_Value().RemoveAt(<>__AnonType.Index);
						}
					}
				}
			}
		}

		private static bool IsIgnorableError(BnetFeatureEvent feature, BattleNetErrors code)
		{
			HashSet<BattleNetErrors> hashSet;
			return BnetParty.s_ignorableErrorCodes.TryGetValue(feature, out hashSet) && hashSet.Contains(code);
		}

		public static void Process()
		{
			PartyListenerEvent[] array;
			BattleNet.GetPartyListenerEvents(out array);
			BattleNet.ClearPartyListenerEvents();
			int i = 0;
			while (i < array.Length)
			{
				PartyListenerEvent partyListenerEvent = array[i];
				PartyId partyId = partyListenerEvent.PartyId;
				switch (partyListenerEvent.Type)
				{
				case PartyListenerEventType.ERROR_RAISED:
				case PartyListenerEventType.OPERATION_CALLBACK:
				{
					PartyError error = partyListenerEvent.ToPartyError();
					if (error.ErrorCode != BattleNetErrors.ERROR_OK)
					{
						if (BnetParty.IsIgnorableError(error.FeatureEvent, error.ErrorCode.EnumVal))
						{
							error.ErrorCode = BattleNetErrors.ERROR_OK;
							if (error.FeatureEvent == BnetFeatureEvent.Party_Leave_Callback)
							{
								if (!BnetParty.s_joinedParties.ContainsKey(partyId))
								{
									BnetParty.s_joinedParties[partyId] = PartyType.SPECTATOR_PARTY;
								}
								goto IL_22D;
							}
						}
						if (error.IsOperationCallback && error.FeatureEvent == BnetFeatureEvent.Party_Create_Callback)
						{
							PartyType partyType = error.PartyType;
							if (BnetParty.s_pendingPartyCreates.ContainsKey(partyType))
							{
								BnetParty.s_pendingPartyCreates.Remove(partyType);
							}
						}
					}
					if (error.ErrorCode != BattleNetErrors.ERROR_OK)
					{
						BnetParty.RaisePartyError(error);
					}
					break;
				}
				case PartyListenerEventType.JOINED_PARTY:
				{
					string stringData = partyListenerEvent.StringData;
					PartyType partyTypeFromString = BnetParty.GetPartyTypeFromString(stringData);
					BnetParty.s_joinedParties[partyId] = partyTypeFromString;
					if (BnetParty.s_pendingPartyCreates != null)
					{
						BnetParty.CreateSuccessCallback createSuccessCallback = null;
						if (BnetParty.s_pendingPartyCreates.ContainsKey(partyTypeFromString))
						{
							createSuccessCallback = BnetParty.s_pendingPartyCreates[partyTypeFromString];
							BnetParty.s_pendingPartyCreates.Remove(partyTypeFromString);
						}
						else if (stringData == "default" && BnetParty.s_pendingPartyCreates.Count == 0)
						{
							createSuccessCallback = Enumerable.First<KeyValuePair<PartyType, BnetParty.CreateSuccessCallback>>(BnetParty.s_pendingPartyCreates).get_Value();
							BnetParty.s_pendingPartyCreates.Clear();
						}
						if (createSuccessCallback != null)
						{
							createSuccessCallback(partyTypeFromString, partyId);
						}
					}
					if (BnetParty.OnJoined != null)
					{
						BnetParty.OnJoined(OnlineEventType.ADDED, new PartyInfo(partyId, partyTypeFromString), default(LeaveReason?));
					}
					break;
				}
				case PartyListenerEventType.LEFT_PARTY:
					goto IL_22D;
				case PartyListenerEventType.PRIVACY_CHANGED:
					if (BnetParty.OnPrivacyLevelChanged != null)
					{
						BnetParty.OnPrivacyLevelChanged(BnetParty.GetJoinedParty(partyId), (PrivacyLevel)partyListenerEvent.UintData);
					}
					break;
				case PartyListenerEventType.MEMBER_JOINED:
				case PartyListenerEventType.MEMBER_LEFT:
					if (BnetParty.OnMemberEvent != null)
					{
						OnlineEventType evt = (partyListenerEvent.Type != PartyListenerEventType.MEMBER_JOINED) ? OnlineEventType.REMOVED : OnlineEventType.ADDED;
						LeaveReason? reason = default(LeaveReason?);
						if (partyListenerEvent.Type == PartyListenerEventType.MEMBER_LEFT)
						{
							reason = new LeaveReason?((LeaveReason)partyListenerEvent.UintData);
						}
						BnetParty.OnMemberEvent(evt, BnetParty.GetJoinedParty(partyId), partyListenerEvent.SubjectMemberId, false, reason);
					}
					break;
				case PartyListenerEventType.MEMBER_ROLE_CHANGED:
					if (BnetParty.OnMemberEvent != null)
					{
						BnetParty.OnMemberEvent(OnlineEventType.UPDATED, BnetParty.GetJoinedParty(partyId), partyListenerEvent.SubjectMemberId, true, default(LeaveReason?));
					}
					break;
				case PartyListenerEventType.RECEIVED_INVITE_ADDED:
				case PartyListenerEventType.RECEIVED_INVITE_REMOVED:
					if (BnetParty.OnReceivedInvite != null)
					{
						OnlineEventType evt2 = (partyListenerEvent.Type != PartyListenerEventType.RECEIVED_INVITE_ADDED) ? OnlineEventType.REMOVED : OnlineEventType.ADDED;
						PartyType type = PartyType.DEFAULT;
						if (partyListenerEvent.StringData != null)
						{
							EnumUtils.TryGetEnum<PartyType>(partyListenerEvent.StringData, out type);
						}
						PartyInfo party = new PartyInfo(partyId, type);
						InviteRemoveReason? reason2 = default(InviteRemoveReason?);
						if (partyListenerEvent.Type == PartyListenerEventType.RECEIVED_INVITE_REMOVED)
						{
							reason2 = new InviteRemoveReason?((InviteRemoveReason)partyListenerEvent.UintData);
						}
						BnetParty.OnReceivedInvite(evt2, party, partyListenerEvent.UlongData, reason2);
					}
					break;
				case PartyListenerEventType.PARTY_INVITE_SENT:
				case PartyListenerEventType.PARTY_INVITE_REMOVED:
					if (BnetParty.OnSentInvite != null)
					{
						bool senderIsMyself = partyListenerEvent.SubjectMemberId == BnetGameAccountId.CreateFromEntityId(BattleNet.GetMyGameAccountId());
						OnlineEventType evt3 = (partyListenerEvent.Type != PartyListenerEventType.PARTY_INVITE_SENT) ? OnlineEventType.REMOVED : OnlineEventType.ADDED;
						PartyType type2 = PartyType.DEFAULT;
						if (partyListenerEvent.StringData != null)
						{
							EnumUtils.TryGetEnum<PartyType>(partyListenerEvent.StringData, out type2);
						}
						PartyInfo party2 = new PartyInfo(partyId, type2);
						InviteRemoveReason? reason3 = default(InviteRemoveReason?);
						if (partyListenerEvent.Type == PartyListenerEventType.PARTY_INVITE_REMOVED)
						{
							reason3 = new InviteRemoveReason?((InviteRemoveReason)partyListenerEvent.UintData);
						}
						BnetParty.OnSentInvite(evt3, party2, partyListenerEvent.UlongData, senderIsMyself, reason3);
					}
					break;
				case PartyListenerEventType.INVITE_REQUEST_ADDED:
				case PartyListenerEventType.INVITE_REQUEST_REMOVED:
					if (BnetParty.OnSentInvite != null)
					{
						OnlineEventType evt4 = (partyListenerEvent.Type != PartyListenerEventType.INVITE_REQUEST_ADDED) ? OnlineEventType.REMOVED : OnlineEventType.ADDED;
						PartyInfo joinedParty = BnetParty.GetJoinedParty(partyId);
						InviteRequestRemovedReason? reason4 = default(InviteRequestRemovedReason?);
						if (partyListenerEvent.Type == PartyListenerEventType.INVITE_REQUEST_REMOVED)
						{
							reason4 = new InviteRequestRemovedReason?((InviteRequestRemovedReason)partyListenerEvent.UintData);
						}
						InviteRequest inviteRequest = new InviteRequest();
						inviteRequest.TargetId = partyListenerEvent.TargetMemberId;
						inviteRequest.TargetName = partyListenerEvent.StringData2;
						inviteRequest.RequesterId = partyListenerEvent.SubjectMemberId;
						inviteRequest.RequesterName = partyListenerEvent.StringData;
						BnetParty.OnReceivedInviteRequest(evt4, joinedParty, inviteRequest, reason4);
					}
					break;
				case PartyListenerEventType.CHAT_MESSAGE_RECEIVED:
					if (BnetParty.OnChatMessage != null)
					{
						BnetParty.OnChatMessage(BnetParty.GetJoinedParty(partyId), partyListenerEvent.SubjectMemberId, partyListenerEvent.StringData);
					}
					break;
				case PartyListenerEventType.PARTY_ATTRIBUTE_CHANGED:
				{
					PartyInfo joinedParty2 = BnetParty.GetJoinedParty(partyId);
					string stringData2 = partyListenerEvent.StringData;
					if (stringData2 == "WTCG.Party.Type")
					{
						string partyAttributeString = BnetParty.GetPartyAttributeString(partyId, "WTCG.Party.Type");
						PartyType partyTypeFromString2 = BnetParty.GetPartyTypeFromString(partyAttributeString);
						if (partyTypeFromString2 != PartyType.DEFAULT)
						{
							BnetParty.s_joinedParties[partyId] = partyTypeFromString2;
						}
					}
					Variant variant = null;
					switch (partyListenerEvent.UintData)
					{
					case 1u:
						variant = new Variant();
						variant.IntValue = (long)partyListenerEvent.UlongData;
						break;
					case 2u:
						variant = new Variant();
						variant.StringValue = partyListenerEvent.StringData2;
						break;
					case 3u:
						variant = new Variant();
						variant.BlobValue = partyListenerEvent.BlobData;
						break;
					}
					IL_611:
					if (BnetParty.OnPartyAttributeChanged != null)
					{
						BnetParty.OnPartyAttributeChanged(joinedParty2, stringData2, variant);
					}
					List<BnetParty.PartyAttributeChangedHandler> list;
					if (BnetParty.s_attributeChangedSubscribers != null && BnetParty.s_attributeChangedSubscribers.TryGetValue(stringData2, out list))
					{
						BnetParty.PartyAttributeChangedHandler[] array2 = list.ToArray();
						BnetParty.PartyAttributeChangedHandler[] array3 = array2;
						for (int j = 0; j < array3.Length; j++)
						{
							BnetParty.PartyAttributeChangedHandler partyAttributeChangedHandler = array3[j];
							partyAttributeChangedHandler(joinedParty2, stringData2, variant);
						}
					}
					break;
					goto IL_611;
				}
				}
				IL_68C:
				i++;
				continue;
				IL_22D:
				if (BnetParty.s_joinedParties.ContainsKey(partyId))
				{
					PartyType type3 = BnetParty.s_joinedParties[partyId];
					BnetParty.s_joinedParties.Remove(partyId);
					if (BnetParty.OnJoined != null)
					{
						BnetParty.OnJoined(OnlineEventType.REMOVED, new PartyInfo(partyId, type3), new LeaveReason?((LeaveReason)partyListenerEvent.UintData));
					}
				}
				goto IL_68C;
			}
		}

		private static void RaisePartyError(bool isOperationCallback, string szPartyType, BnetFeatureEvent featureEvent, string errorMessageFormat, params object[] args)
		{
			string debugContext = string.Format(errorMessageFormat, args);
			BnetParty.RaisePartyError(new PartyError
			{
				IsOperationCallback = isOperationCallback,
				DebugContext = debugContext,
				ErrorCode = BattleNetErrors.ERROR_OK,
				Feature = BnetFeature.Party,
				FeatureEvent = featureEvent,
				szPartyType = szPartyType
			});
		}

		private static void RaisePartyError(bool isOperationCallback, string debugContext, BattleNetErrors errorCode, BnetFeature feature, BnetFeatureEvent featureEvent, PartyId partyId, string szPartyType, string stringData, string errorMessageFormat, params object[] args)
		{
			if (BnetParty.OnError == null)
			{
				return;
			}
			BnetParty.RaisePartyError(new PartyError
			{
				IsOperationCallback = isOperationCallback,
				DebugContext = debugContext,
				ErrorCode = errorCode,
				Feature = feature,
				FeatureEvent = featureEvent,
				PartyId = partyId,
				szPartyType = szPartyType,
				StringData = stringData
			});
		}

		private static void RaisePartyError(PartyError error)
		{
			string message = string.Format("BnetParty: event={0} feature={1} code={2} partyId={3} type={4} strData={5}", new object[]
			{
				error.FeatureEvent.ToString(),
				(int)error.FeatureEvent,
				error.ErrorCode,
				error.PartyId,
				error.szPartyType,
				error.StringData
			});
			LogLevel level = (!(error.ErrorCode == BattleNetErrors.ERROR_OK)) ? LogLevel.Error : LogLevel.Info;
			Log.Party.Print(level, message);
			if (BnetParty.OnError != null)
			{
				BnetParty.OnError(error);
			}
		}

		public static PartyType GetPartyTypeFromString(string partyType)
		{
			PartyType result = PartyType.DEFAULT;
			if (partyType != null)
			{
				EnumUtils.TryGetEnum<PartyType>(partyType, out result);
			}
			return result;
		}
	}
}
