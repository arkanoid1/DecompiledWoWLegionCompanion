using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bnet.protocol.account
{
	public class CacheExpireRequest : IProtoBuf
	{
		private List<AccountId> _Account = new List<AccountId>();

		private List<GameAccountHandle> _GameAccount = new List<GameAccountHandle>();

		private List<string> _Email = new List<string>();

		public List<AccountId> Account
		{
			get
			{
				return this._Account;
			}
			set
			{
				this._Account = value;
			}
		}

		public List<AccountId> AccountList
		{
			get
			{
				return this._Account;
			}
		}

		public int AccountCount
		{
			get
			{
				return this._Account.get_Count();
			}
		}

		public List<GameAccountHandle> GameAccount
		{
			get
			{
				return this._GameAccount;
			}
			set
			{
				this._GameAccount = value;
			}
		}

		public List<GameAccountHandle> GameAccountList
		{
			get
			{
				return this._GameAccount;
			}
		}

		public int GameAccountCount
		{
			get
			{
				return this._GameAccount.get_Count();
			}
		}

		public List<string> Email
		{
			get
			{
				return this._Email;
			}
			set
			{
				this._Email = value;
			}
		}

		public List<string> EmailList
		{
			get
			{
				return this._Email;
			}
		}

		public int EmailCount
		{
			get
			{
				return this._Email.get_Count();
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
			CacheExpireRequest.Deserialize(stream, this);
		}

		public static CacheExpireRequest Deserialize(Stream stream, CacheExpireRequest instance)
		{
			return CacheExpireRequest.Deserialize(stream, instance, -1L);
		}

		public static CacheExpireRequest DeserializeLengthDelimited(Stream stream)
		{
			CacheExpireRequest cacheExpireRequest = new CacheExpireRequest();
			CacheExpireRequest.DeserializeLengthDelimited(stream, cacheExpireRequest);
			return cacheExpireRequest;
		}

		public static CacheExpireRequest DeserializeLengthDelimited(Stream stream, CacheExpireRequest instance)
		{
			long num = (long)((ulong)ProtocolParser.ReadUInt32(stream));
			num += stream.get_Position();
			return CacheExpireRequest.Deserialize(stream, instance, num);
		}

		public static CacheExpireRequest Deserialize(Stream stream, CacheExpireRequest instance, long limit)
		{
			if (instance.Account == null)
			{
				instance.Account = new List<AccountId>();
			}
			if (instance.GameAccount == null)
			{
				instance.GameAccount = new List<GameAccountHandle>();
			}
			if (instance.Email == null)
			{
				instance.Email = new List<string>();
			}
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
								instance.Email.Add(ProtocolParser.ReadString(stream));
							}
						}
						else
						{
							instance.GameAccount.Add(GameAccountHandle.DeserializeLengthDelimited(stream));
						}
					}
					else
					{
						instance.Account.Add(AccountId.DeserializeLengthDelimited(stream));
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
			CacheExpireRequest.Serialize(stream, this);
		}

		public static void Serialize(Stream stream, CacheExpireRequest instance)
		{
			if (instance.Account.get_Count() > 0)
			{
				using (List<AccountId>.Enumerator enumerator = instance.Account.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						AccountId current = enumerator.get_Current();
						stream.WriteByte(10);
						ProtocolParser.WriteUInt32(stream, current.GetSerializedSize());
						AccountId.Serialize(stream, current);
					}
				}
			}
			if (instance.GameAccount.get_Count() > 0)
			{
				using (List<GameAccountHandle>.Enumerator enumerator2 = instance.GameAccount.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						GameAccountHandle current2 = enumerator2.get_Current();
						stream.WriteByte(18);
						ProtocolParser.WriteUInt32(stream, current2.GetSerializedSize());
						GameAccountHandle.Serialize(stream, current2);
					}
				}
			}
			if (instance.Email.get_Count() > 0)
			{
				using (List<string>.Enumerator enumerator3 = instance.Email.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						string current3 = enumerator3.get_Current();
						stream.WriteByte(26);
						ProtocolParser.WriteBytes(stream, Encoding.get_UTF8().GetBytes(current3));
					}
				}
			}
		}

		public uint GetSerializedSize()
		{
			uint num = 0u;
			if (this.Account.get_Count() > 0)
			{
				using (List<AccountId>.Enumerator enumerator = this.Account.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						AccountId current = enumerator.get_Current();
						num += 1u;
						uint serializedSize = current.GetSerializedSize();
						num += serializedSize + ProtocolParser.SizeOfUInt32(serializedSize);
					}
				}
			}
			if (this.GameAccount.get_Count() > 0)
			{
				using (List<GameAccountHandle>.Enumerator enumerator2 = this.GameAccount.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						GameAccountHandle current2 = enumerator2.get_Current();
						num += 1u;
						uint serializedSize2 = current2.GetSerializedSize();
						num += serializedSize2 + ProtocolParser.SizeOfUInt32(serializedSize2);
					}
				}
			}
			if (this.Email.get_Count() > 0)
			{
				using (List<string>.Enumerator enumerator3 = this.Email.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						string current3 = enumerator3.get_Current();
						num += 1u;
						uint byteCount = (uint)Encoding.get_UTF8().GetByteCount(current3);
						num += ProtocolParser.SizeOfUInt32(byteCount) + byteCount;
					}
				}
			}
			return num;
		}

		public void AddAccount(AccountId val)
		{
			this._Account.Add(val);
		}

		public void ClearAccount()
		{
			this._Account.Clear();
		}

		public void SetAccount(List<AccountId> val)
		{
			this.Account = val;
		}

		public void AddGameAccount(GameAccountHandle val)
		{
			this._GameAccount.Add(val);
		}

		public void ClearGameAccount()
		{
			this._GameAccount.Clear();
		}

		public void SetGameAccount(List<GameAccountHandle> val)
		{
			this.GameAccount = val;
		}

		public void AddEmail(string val)
		{
			this._Email.Add(val);
		}

		public void ClearEmail()
		{
			this._Email.Clear();
		}

		public void SetEmail(List<string> val)
		{
			this.Email = val;
		}

		public override int GetHashCode()
		{
			int num = base.GetType().GetHashCode();
			using (List<AccountId>.Enumerator enumerator = this.Account.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					AccountId current = enumerator.get_Current();
					num ^= current.GetHashCode();
				}
			}
			using (List<GameAccountHandle>.Enumerator enumerator2 = this.GameAccount.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					GameAccountHandle current2 = enumerator2.get_Current();
					num ^= current2.GetHashCode();
				}
			}
			using (List<string>.Enumerator enumerator3 = this.Email.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					string current3 = enumerator3.get_Current();
					num ^= current3.GetHashCode();
				}
			}
			return num;
		}

		public override bool Equals(object obj)
		{
			CacheExpireRequest cacheExpireRequest = obj as CacheExpireRequest;
			if (cacheExpireRequest == null)
			{
				return false;
			}
			if (this.Account.get_Count() != cacheExpireRequest.Account.get_Count())
			{
				return false;
			}
			for (int i = 0; i < this.Account.get_Count(); i++)
			{
				if (!this.Account.get_Item(i).Equals(cacheExpireRequest.Account.get_Item(i)))
				{
					return false;
				}
			}
			if (this.GameAccount.get_Count() != cacheExpireRequest.GameAccount.get_Count())
			{
				return false;
			}
			for (int j = 0; j < this.GameAccount.get_Count(); j++)
			{
				if (!this.GameAccount.get_Item(j).Equals(cacheExpireRequest.GameAccount.get_Item(j)))
				{
					return false;
				}
			}
			if (this.Email.get_Count() != cacheExpireRequest.Email.get_Count())
			{
				return false;
			}
			for (int k = 0; k < this.Email.get_Count(); k++)
			{
				if (!this.Email.get_Item(k).Equals(cacheExpireRequest.Email.get_Item(k)))
				{
					return false;
				}
			}
			return true;
		}

		public static CacheExpireRequest ParseFrom(byte[] bs)
		{
			return ProtobufUtil.ParseFrom<CacheExpireRequest>(bs, 0, -1);
		}
	}
}
