using System;
using System.Collections.Generic;
using System.IO;

namespace bnet.protocol.account
{
	public class GameAccountList : IProtoBuf
	{
		public bool HasRegion;

		private uint _Region;

		private List<GameAccountHandle> _Handle = new List<GameAccountHandle>();

		public uint Region
		{
			get
			{
				return this._Region;
			}
			set
			{
				this._Region = value;
				this.HasRegion = true;
			}
		}

		public List<GameAccountHandle> Handle
		{
			get
			{
				return this._Handle;
			}
			set
			{
				this._Handle = value;
			}
		}

		public List<GameAccountHandle> HandleList
		{
			get
			{
				return this._Handle;
			}
		}

		public int HandleCount
		{
			get
			{
				return this._Handle.get_Count();
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
			GameAccountList.Deserialize(stream, this);
		}

		public static GameAccountList Deserialize(Stream stream, GameAccountList instance)
		{
			return GameAccountList.Deserialize(stream, instance, -1L);
		}

		public static GameAccountList DeserializeLengthDelimited(Stream stream)
		{
			GameAccountList gameAccountList = new GameAccountList();
			GameAccountList.DeserializeLengthDelimited(stream, gameAccountList);
			return gameAccountList;
		}

		public static GameAccountList DeserializeLengthDelimited(Stream stream, GameAccountList instance)
		{
			long num = (long)((ulong)ProtocolParser.ReadUInt32(stream));
			num += stream.get_Position();
			return GameAccountList.Deserialize(stream, instance, num);
		}

		public static GameAccountList Deserialize(Stream stream, GameAccountList instance, long limit)
		{
			if (instance.Handle == null)
			{
				instance.Handle = new List<GameAccountHandle>();
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
					if (num2 != 24)
					{
						if (num2 != 34)
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
							instance.Handle.Add(GameAccountHandle.DeserializeLengthDelimited(stream));
						}
					}
					else
					{
						instance.Region = ProtocolParser.ReadUInt32(stream);
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
			GameAccountList.Serialize(stream, this);
		}

		public static void Serialize(Stream stream, GameAccountList instance)
		{
			if (instance.HasRegion)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt32(stream, instance.Region);
			}
			if (instance.Handle.get_Count() > 0)
			{
				using (List<GameAccountHandle>.Enumerator enumerator = instance.Handle.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GameAccountHandle current = enumerator.get_Current();
						stream.WriteByte(34);
						ProtocolParser.WriteUInt32(stream, current.GetSerializedSize());
						GameAccountHandle.Serialize(stream, current);
					}
				}
			}
		}

		public uint GetSerializedSize()
		{
			uint num = 0u;
			if (this.HasRegion)
			{
				num += 1u;
				num += ProtocolParser.SizeOfUInt32(this.Region);
			}
			if (this.Handle.get_Count() > 0)
			{
				using (List<GameAccountHandle>.Enumerator enumerator = this.Handle.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GameAccountHandle current = enumerator.get_Current();
						num += 1u;
						uint serializedSize = current.GetSerializedSize();
						num += serializedSize + ProtocolParser.SizeOfUInt32(serializedSize);
					}
				}
			}
			return num;
		}

		public void SetRegion(uint val)
		{
			this.Region = val;
		}

		public void AddHandle(GameAccountHandle val)
		{
			this._Handle.Add(val);
		}

		public void ClearHandle()
		{
			this._Handle.Clear();
		}

		public void SetHandle(List<GameAccountHandle> val)
		{
			this.Handle = val;
		}

		public override int GetHashCode()
		{
			int num = base.GetType().GetHashCode();
			if (this.HasRegion)
			{
				num ^= this.Region.GetHashCode();
			}
			using (List<GameAccountHandle>.Enumerator enumerator = this.Handle.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GameAccountHandle current = enumerator.get_Current();
					num ^= current.GetHashCode();
				}
			}
			return num;
		}

		public override bool Equals(object obj)
		{
			GameAccountList gameAccountList = obj as GameAccountList;
			if (gameAccountList == null)
			{
				return false;
			}
			if (this.HasRegion != gameAccountList.HasRegion || (this.HasRegion && !this.Region.Equals(gameAccountList.Region)))
			{
				return false;
			}
			if (this.Handle.get_Count() != gameAccountList.Handle.get_Count())
			{
				return false;
			}
			for (int i = 0; i < this.Handle.get_Count(); i++)
			{
				if (!this.Handle.get_Item(i).Equals(gameAccountList.Handle.get_Item(i)))
				{
					return false;
				}
			}
			return true;
		}

		public static GameAccountList ParseFrom(byte[] bs)
		{
			return ProtobufUtil.ParseFrom<GameAccountList>(bs, 0, -1);
		}
	}
}
