using System;
using System.Collections.Generic;
using System.IO;

namespace bnet.protocol.channel
{
	public class JoinChannelRequest : IProtoBuf
	{
		public bool HasAgentIdentity;

		private Identity _AgentIdentity;

		public bool HasMemberState;

		private MemberState _MemberState;

		private List<EntityId> _FriendAccountId = new List<EntityId>();

		public bool HasLocalSubscriber;

		private bool _LocalSubscriber;

		public Identity AgentIdentity
		{
			get
			{
				return this._AgentIdentity;
			}
			set
			{
				this._AgentIdentity = value;
				this.HasAgentIdentity = (value != null);
			}
		}

		public MemberState MemberState
		{
			get
			{
				return this._MemberState;
			}
			set
			{
				this._MemberState = value;
				this.HasMemberState = (value != null);
			}
		}

		public EntityId ChannelId
		{
			get;
			set;
		}

		public ulong ObjectId
		{
			get;
			set;
		}

		public List<EntityId> FriendAccountId
		{
			get
			{
				return this._FriendAccountId;
			}
			set
			{
				this._FriendAccountId = value;
			}
		}

		public List<EntityId> FriendAccountIdList
		{
			get
			{
				return this._FriendAccountId;
			}
		}

		public int FriendAccountIdCount
		{
			get
			{
				return this._FriendAccountId.get_Count();
			}
		}

		public bool LocalSubscriber
		{
			get
			{
				return this._LocalSubscriber;
			}
			set
			{
				this._LocalSubscriber = value;
				this.HasLocalSubscriber = true;
			}
		}

		public bool IsInitialized
		{
			get
			{
				return true;
			}
		}

		public void Deserialize(Stream stream)
		{
			JoinChannelRequest.Deserialize(stream, this);
		}

		public static JoinChannelRequest Deserialize(Stream stream, JoinChannelRequest instance)
		{
			return JoinChannelRequest.Deserialize(stream, instance, -1L);
		}

		public static JoinChannelRequest DeserializeLengthDelimited(Stream stream)
		{
			JoinChannelRequest joinChannelRequest = new JoinChannelRequest();
			JoinChannelRequest.DeserializeLengthDelimited(stream, joinChannelRequest);
			return joinChannelRequest;
		}

		public static JoinChannelRequest DeserializeLengthDelimited(Stream stream, JoinChannelRequest instance)
		{
			long num = (long)((ulong)ProtocolParser.ReadUInt32(stream));
			num += stream.get_Position();
			return JoinChannelRequest.Deserialize(stream, instance, num);
		}

		public static JoinChannelRequest Deserialize(Stream stream, JoinChannelRequest instance, long limit)
		{
			if (instance.FriendAccountId == null)
			{
				instance.FriendAccountId = new List<EntityId>();
			}
			instance.LocalSubscriber = true;
			while (limit < 0L || stream.get_Position() < limit)
			{
				int num = stream.ReadByte();
				if (num == -1)
				{
					if (limit >= 0L)
					{
						throw new EndOfStreamException();
					}
					return instance;
				}
				else
				{
					int num2 = num;
					if (num2 != 10)
					{
						if (num2 != 18)
						{
							if (num2 != 26)
							{
								if (num2 != 32)
								{
									if (num2 != 42)
									{
										if (num2 != 48)
										{
											Key key = ProtocolParser.ReadKey((byte)num, stream);
											uint field = key.Field;
											if (field == 0u)
											{
												throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
											}
											ProtocolParser.SkipKey(stream, key);
										}
										else
										{
											instance.LocalSubscriber = ProtocolParser.ReadBool(stream);
										}
									}
									else
									{
										instance.FriendAccountId.Add(EntityId.DeserializeLengthDelimited(stream));
									}
								}
								else
								{
									instance.ObjectId = ProtocolParser.ReadUInt64(stream);
								}
							}
							else if (instance.ChannelId == null)
							{
								instance.ChannelId = EntityId.DeserializeLengthDelimited(stream);
							}
							else
							{
								EntityId.DeserializeLengthDelimited(stream, instance.ChannelId);
							}
						}
						else if (instance.MemberState == null)
						{
							instance.MemberState = MemberState.DeserializeLengthDelimited(stream);
						}
						else
						{
							MemberState.DeserializeLengthDelimited(stream, instance.MemberState);
						}
					}
					else if (instance.AgentIdentity == null)
					{
						instance.AgentIdentity = Identity.DeserializeLengthDelimited(stream);
					}
					else
					{
						Identity.DeserializeLengthDelimited(stream, instance.AgentIdentity);
					}
				}
			}
			if (stream.get_Position() == limit)
			{
				return instance;
			}
			throw new ProtocolBufferException("Read past max limit");
		}

		public void Serialize(Stream stream)
		{
			JoinChannelRequest.Serialize(stream, this);
		}

		public static void Serialize(Stream stream, JoinChannelRequest instance)
		{
			if (instance.HasAgentIdentity)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteUInt32(stream, instance.AgentIdentity.GetSerializedSize());
				Identity.Serialize(stream, instance.AgentIdentity);
			}
			if (instance.HasMemberState)
			{
				stream.WriteByte(18);
				ProtocolParser.WriteUInt32(stream, instance.MemberState.GetSerializedSize());
				MemberState.Serialize(stream, instance.MemberState);
			}
			if (instance.ChannelId == null)
			{
				throw new ArgumentNullException("ChannelId", "Required by proto specification.");
			}
			stream.WriteByte(26);
			ProtocolParser.WriteUInt32(stream, instance.ChannelId.GetSerializedSize());
			EntityId.Serialize(stream, instance.ChannelId);
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.ObjectId);
			if (instance.FriendAccountId.get_Count() > 0)
			{
				using (List<EntityId>.Enumerator enumerator = instance.FriendAccountId.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						EntityId current = enumerator.get_Current();
						stream.WriteByte(42);
						ProtocolParser.WriteUInt32(stream, current.GetSerializedSize());
						EntityId.Serialize(stream, current);
					}
				}
			}
			if (instance.HasLocalSubscriber)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteBool(stream, instance.LocalSubscriber);
			}
		}

		public uint GetSerializedSize()
		{
			uint num = 0u;
			if (this.HasAgentIdentity)
			{
				num += 1u;
				uint serializedSize = this.AgentIdentity.GetSerializedSize();
				num += serializedSize + ProtocolParser.SizeOfUInt32(serializedSize);
			}
			if (this.HasMemberState)
			{
				num += 1u;
				uint serializedSize2 = this.MemberState.GetSerializedSize();
				num += serializedSize2 + ProtocolParser.SizeOfUInt32(serializedSize2);
			}
			uint serializedSize3 = this.ChannelId.GetSerializedSize();
			num += serializedSize3 + ProtocolParser.SizeOfUInt32(serializedSize3);
			num += ProtocolParser.SizeOfUInt64(this.ObjectId);
			if (this.FriendAccountId.get_Count() > 0)
			{
				using (List<EntityId>.Enumerator enumerator = this.FriendAccountId.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						EntityId current = enumerator.get_Current();
						num += 1u;
						uint serializedSize4 = current.GetSerializedSize();
						num += serializedSize4 + ProtocolParser.SizeOfUInt32(serializedSize4);
					}
				}
			}
			if (this.HasLocalSubscriber)
			{
				num += 1u;
				num += 1u;
			}
			num += 2u;
			return num;
		}

		public void SetAgentIdentity(Identity val)
		{
			this.AgentIdentity = val;
		}

		public void SetMemberState(MemberState val)
		{
			this.MemberState = val;
		}

		public void SetChannelId(EntityId val)
		{
			this.ChannelId = val;
		}

		public void SetObjectId(ulong val)
		{
			this.ObjectId = val;
		}

		public void AddFriendAccountId(EntityId val)
		{
			this._FriendAccountId.Add(val);
		}

		public void ClearFriendAccountId()
		{
			this._FriendAccountId.Clear();
		}

		public void SetFriendAccountId(List<EntityId> val)
		{
			this.FriendAccountId = val;
		}

		public void SetLocalSubscriber(bool val)
		{
			this.LocalSubscriber = val;
		}

		public override int GetHashCode()
		{
			int num = base.GetType().GetHashCode();
			if (this.HasAgentIdentity)
			{
				num ^= this.AgentIdentity.GetHashCode();
			}
			if (this.HasMemberState)
			{
				num ^= this.MemberState.GetHashCode();
			}
			num ^= this.ChannelId.GetHashCode();
			num ^= this.ObjectId.GetHashCode();
			using (List<EntityId>.Enumerator enumerator = this.FriendAccountId.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					EntityId current = enumerator.get_Current();
					num ^= current.GetHashCode();
				}
			}
			if (this.HasLocalSubscriber)
			{
				num ^= this.LocalSubscriber.GetHashCode();
			}
			return num;
		}

		public override bool Equals(object obj)
		{
			JoinChannelRequest joinChannelRequest = obj as JoinChannelRequest;
			if (joinChannelRequest == null)
			{
				return false;
			}
			if (this.HasAgentIdentity != joinChannelRequest.HasAgentIdentity || (this.HasAgentIdentity && !this.AgentIdentity.Equals(joinChannelRequest.AgentIdentity)))
			{
				return false;
			}
			if (this.HasMemberState != joinChannelRequest.HasMemberState || (this.HasMemberState && !this.MemberState.Equals(joinChannelRequest.MemberState)))
			{
				return false;
			}
			if (!this.ChannelId.Equals(joinChannelRequest.ChannelId))
			{
				return false;
			}
			if (!this.ObjectId.Equals(joinChannelRequest.ObjectId))
			{
				return false;
			}
			if (this.FriendAccountId.get_Count() != joinChannelRequest.FriendAccountId.get_Count())
			{
				return false;
			}
			for (int i = 0; i < this.FriendAccountId.get_Count(); i++)
			{
				if (!this.FriendAccountId.get_Item(i).Equals(joinChannelRequest.FriendAccountId.get_Item(i)))
				{
					return false;
				}
			}
			return this.HasLocalSubscriber == joinChannelRequest.HasLocalSubscriber && (!this.HasLocalSubscriber || this.LocalSubscriber.Equals(joinChannelRequest.LocalSubscriber));
		}

		public static JoinChannelRequest ParseFrom(byte[] bs)
		{
			return ProtobufUtil.ParseFrom<JoinChannelRequest>(bs, 0, -1);
		}
	}
}
