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
    /// 玩家手牌的数量上限
    /// </summary>
    public static int maxHandSize = 17;
    /// <summary>
    /// 出牌的所有类型
    /// </summary>
    public enum CardType
    {
        /// <summary>
        /// 单张
        /// </summary>
        SINGLE,
        /// <summary>
        /// 对子
        /// </summary>
        PAIR,
        /// <summary>
        /// 三张
        /// </summary>
        TRIPLE,
        /// <summary>
        /// 三带一
        /// </summary>
        TRIPLE_WITH_SINGLE,
        /// <summary>
        /// 三带二
        /// </summary>
        TRIPLE_WITH_PAIR,
        /// <summary>
        /// 顺子
        /// </summary>
        STRAIGHT,
        /// <summary>
        /// 连对
        /// </summary>
        STRAIGHT_PAIRS,
        /// <summary>
        /// 飞机
        /// </summary>
        AIRPLANE,
        /// <summary>
        /// 飞机带单张，333 444 5 6
        /// </summary>
        AIRPLANE_WITH_SINGLES,
        /// <summary>
        /// 飞机带对子，333 444 55 66
        /// </summary>
        AIRPLANE_WITH_PAIRS,
        /// <summary>
        /// 炸弹
        /// </summary>
        BOMB,
        /// <summary>
        /// 王炸
        /// </summary>
        JOKER_BOMB,
        /// <summary>
        /// 四代二
        /// </summary>
        FOUR_WITH_TWO,
        /// <summary>
        /// 无效出牌
        /// </summary>
        INVALID,
    }

    /// <summary>
    /// 生成一副扑克牌
    /// </summary>
    public static void CreatePoker()
    {
        cards.Clear();

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

    /// <summary>
    /// Card数组转成CardInfo数组
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    public static CardInfo[] GetCardInfos(Card[] cards)
    {
        CardInfo[] cardInfos = new CardInfo[cards.Length];
        for (int i = 0; i < cardInfos.Length; i++)
        {
            cardInfos[i] = cards[i].GetCardInfo();
        }
        return cardInfos;
    }
}

