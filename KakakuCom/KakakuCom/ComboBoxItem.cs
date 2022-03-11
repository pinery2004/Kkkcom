using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kc
{
	class ComboBoxItem
	{
        private string m_id = "";
        private string m_name = "";

        //コンストラクタ
        public ComboBoxItem(string id, string name)
        {
            m_id = id;
            m_name = name;
        }

        //実際の値
        public string Id
        {
            get
            {
                return m_id;
            }
        }

       //表示名称
         //(このプロパティはこのサンプルでは使わないのでなくても良い)
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        //オーバーライドしたメソッド
          //これがコンボボックスに表示される
        public override string ToString()
        {
            return m_name;
        }
	}
}
