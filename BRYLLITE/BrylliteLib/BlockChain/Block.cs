using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.BlockChain
{
    public class Block
    {
        // 블록 헤더
        private BlockHeader _header = null;

        // 트랜잭션 목록
        private Transactions _transactions = null;

        public BlockHeader Header => _header;

        public Transactions Transactions => _transactions;

        public byte[] Hash
        {
            get
            {
                return Header.Hash;
            }
        }

        public Block(long height)
        {
            _header = new BlockHeader(height);
            _transactions = new Transactions();
        }
    }
}
