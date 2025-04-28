using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CardManager
{
    /// <summary>
    /// 存放卡牌的集合
    /// </summary>
    public static List<Card> cards = new List<Card>();

    /// <summary>
    /// 生成一副扑克牌
    /// </summary>
    public static void CreatePoker()
    {
        //四种花色
        for(int i = 1; i <= 4; i++)
        {
            //13种牌面大小
            for(int j = 0; j < 13; j++)
            {
                Card card = new Card(i, j);
                cards.Add(card);
            }
        }

        //添加大小王
        cards.Add(new Card(Suit.None, Rank.SJoker));
        cards.Add(new Card(Suit.None, Rank.LJoker));
    }

    /// <summary>
    /// 洗牌
    /// </summary>
    public static void Shuffle()
    {
        //先生成一副扑克牌
        CreatePoker();

        //创建一个队列并洗牌
        Queue<Card> cardQueue = new Queue<Card>();
        int size = cards.Count;
        for(int i = 0; i < size; i++)
        {
            Random random = new Random();
            int index = random.Next(cards.Count);
            cardQueue.Enqueue(cards[index]);
            cards.RemoveAt(index);
        }

        //将洗好的牌放回到卡牌列表中
        for(int i = 0; i < size; i++)
        {
            cards.Add(cardQueue.Dequeue());
        }
    }
}

