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

    /// <summary>
    /// CardInfo数组转成Card数组
    /// </summary>
    /// <param name="cardInfos"></param>
    /// <returns></returns>
    public static Card[] GetCards(CardInfo[] cardInfos)
    {
        Card[] cards = new Card[cardInfos.Length];
        for(int i = 0;i < cards.Length; i++)
        {
            cards[i] = new Card(cardInfos[i].suit, cardInfos[i].rank);
        }
        return cards;
    }

    /// <summary>
    /// 获取出牌的类型
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    public static CardType GetCardType(Card[] cards)
    {
        //出牌与花色无关，只关心具体大小
        int[] rank = new int[20];
        int cardLen = cards.Length;
        for (int i = 0; i < cardLen; i++)
        {
            rank[i] = (int)cards[i].rank;
        }
        Array.Sort(rank, 0, cardLen);

        //默认为无效
        CardType cardType = CardType.INVALID;

        //开始判断出牌类型
        if(cardLen == 1)
        {
            //只有一张，必然是单张
            cardType = CardType.SINGLE;
        }

        if(cardLen == 2)
        {
            if (rank[0] == rank[1])
            {
                //两张相等，是对子
                cardType = CardType.PAIR;
            }

            if (rank[0] + rank[1] == 27)
            {
                /*
                 * 大小王Rank.SJoker和Rank.LJoker的权值分别为13 14，且只有一张
                 * 所以如果是和为27，说明打出了王炸
                */
                cardType = CardType.JOKER_BOMB;
            }
        }

        if(cardLen == 3)
        {
            if (rank[0] == rank[1] && rank[0] == rank[2])
            {
                //三张都相等，是三张
                cardType = CardType.TRIPLE;
            }
        }

        if(cardLen == 4)
        {
            //先判断炸弹的情况，剩下的一定有不相等的牌，
            //由于手牌经过排序，所以对于三带一直接按顺序找三张相等的就行
            if (rank[0] == rank[1] && rank[0] == rank[2] && rank[0] == rank[3]) 
            {
                //四张相等的牌，是炸弹
                cardType = CardType.BOMB;
            }
            else if(rank[0] == rank[1] && rank[0] == rank[2])
            {
                //前三张相等的三带一
                cardType = CardType.TRIPLE_WITH_SINGLE;
            }
            else if(rank[1] == rank[2] && rank[1] == rank[3])
            {
                //后三张相等的三带一
                cardType = CardType.TRIPLE_WITH_SINGLE;
            }
        }

        if(cardLen == 5)
        {
            //判断三带二只需要分别判断三张一样和两张一样就行，由于一张牌最多四张，所以不存在五张一样的情况
            if((rank[0] == rank[1] && rank[0] == rank[2]) && (rank[3] == rank[4]))
            {
                cardType = CardType.TRIPLE_WITH_PAIR;
            }
            else if((rank[2] == rank[3] && rank[2] == rank[4]) && (rank[0] == rank[1]))
            {
                cardType = CardType.TRIPLE_WITH_PAIR;
            }
        }

        //判断 顺子 的情况。最多可以3-A一起出
        if(cardLen >=5 && cardLen <= 12)
        {
            bool isStraight = true;
            for (int i = 0; i < cardLen - 1; i++) 
            {
                //Rank.Ace的权值为11，超过他的牌不能在顺子里面出
                //rank[i + 1]也要比较是因为循环只到cardLen - 1的位置
                if (rank[i] > 11 || rank[i + 1] > 11) 
                {
                    isStraight = false;
                    break;
                }

                //如果是顺子，那么经排序后，rank[i] - rank[i+1]一定等于-1
                if (rank[i] - rank[i+1] != -1)
                {
                    isStraight = false;
                    break;
                }
            }

            if (isStraight)
            {
                cardType = CardType.STRAIGHT;
            }
        }

        //判断 连对 的情况。连对至少三个对子且没有上限，一次20张都行。
        if(cardLen >=6 && cardLen % 2 == 0)
        {
            bool isStraightPair = true;

            for (int i = 0; i < cardLen; i+=2)
            {
                if (rank[i] != rank[i + 1])
                {
                    isStraightPair = false;
                    break;
                }

                //Rank.Ace的权值为11，超过他的牌不能在连对里面出
                if (rank[i] > 11)
                {
                    isStraightPair = false;
                    break;
                }
            }

            for(int i = 0;i<cardLen - 2; i += 2)
            {
                if (rank[i] - rank[i+2] != -1)
                {
                    isStraightPair = false;
                    break;
                }
            }

            if (isStraightPair)
            {
                cardType = CardType.STRAIGHT_PAIRS;
            }
        }

        //判断 飞机（不带翅膀） 的情况。
        if(cardLen>=6 && cardLen % 3 == 0)
        {
            bool isAirPlane = true;

            for (int i = 0; i < cardLen; i += 3)
            {
                if (rank[i] != rank[i + 1] || rank[i] != rank[i + 2]) 
                {
                    isAirPlane = false;
                    break;
                }

                //Rank.Ace的权值为11，超过他的牌不能在连对里面出
                if (rank[i] > 11)
                {
                    isAirPlane = false;
                    break;
                }
            }

            for (int i = 0; i < cardLen - 3; i += 3)
            {
                if (rank[i] - rank[i + 3] != -1)
                {
                    isAirPlane = false;
                    break;
                }
            }

            if (isAirPlane)
            {
                cardType = CardType.AIRPLANE;
            }
        }

        //判断 飞机带单翅 的情况。333 444 5 6
        if(cardLen >= 8 && cardLen % 4 == 0)
        {
            bool isAirplaneWithSingles = true;

            /*
             * 333 444 555 6 7 8
             * airplaneLen就是三张的数量
             * 先收集所有的三张，得到3 4 5，判断三张是否连续。
             */
            int airplaneLen = cardLen / 4;
            int index = 0;
            int[] arr = new int[airplaneLen];
            for (int i = 0; i < cardLen - 2; i++) 
            {
                if (rank[i] == rank[i+1] && rank[i] == rank[i + 2])
                {
                    arr[index++] = rank[i];

                    /*
                     * 333 444 4 555 6 7
                     * 上述情况下，如果没有 i += 2 这一步，4会被计算两遍
                     */
                    i += 2;
                }
            }

            if(index == airplaneLen)
            {
                //确保飞机的个数是正确的再往下处理
                for (int i = 0; index < airplaneLen - 1; i++)
                {
                    if (arr[i] > 11 || arr[i + 1] > 11) 
                    {
                        //飞机的三张不能包括比A更大的牌
                        isAirplaneWithSingles =false;
                        break;
                    }
                    if (arr[i] - arr[i+1] != -1)
                    {
                        //飞机的三张不是连续的
                        isAirplaneWithSingles = false;
                        break;
                    }
                }
            }
            else
            {
                isAirplaneWithSingles = false;
            }

            if (isAirplaneWithSingles)
            {
                cardType = CardType.AIRPLANE_WITH_SINGLES;
            }
        }

        //判断 飞机带双翅 的情况。333 444 555 66 77 88
        if(cardLen>=10 && cardLen % 5 == 0)
        {
            bool isAirplaneWithPairs = true;

            int airplaneLen = cardLen / 5;
            int index = 0;
            int[] arr = new int[airplaneLen];
            for (int i = 0; i < cardLen - 2; i++) 
            {
                if (rank[i] == rank[i+1] && rank[i] == rank[i + 2])
                {
                    arr[index++] = rank[i];
                    i += 2;
                }
            }

            if(index == airplaneLen)
            {
                for (int i = 0; i < airplaneLen - 1; i++)
                {
                    if (arr[i] > 11 || arr[i + 1] > 11)
                    {
                        isAirplaneWithPairs = false;
                        break;
                    }

                    if (arr[i] - arr[i + 1] != -1)
                    {
                        isAirplaneWithPairs = false;
                        break;
                    }
                }
            }
            else
            {
                isAirplaneWithPairs = false;
            }

            //再判断剩下的是否成对
            for (int i = 0;i < cardLen - 1; i++)
            {
                if (!arr.Contains(rank[i]))
                {
                    //不包含在arr中，说明不是飞机的三张，继续判断是否是对子
                    if (rank[i] != rank[i + 1])
                    {
                        isAirplaneWithPairs = false;
                    }
                    else
                    {
                        i++;
                    }
                }
            } 

            if (isAirplaneWithPairs)
            {
                cardType = CardType.AIRPLANE_WITH_PAIRS;
            }
        }

        //判断 四带二 的情况
        if(cardLen == 6)
        {
            bool isFourWithTwo = true;

            //四带二只有三种牌
            Dictionary<int,int> dic = new Dictionary<int,int>();
            for(int i = 0; i < cardLen; i++)
            {
                if (dic.ContainsKey(rank[i]))
                {
                    dic[rank[i]]++;
                }
                else
                {
                    dic.Add(rank[i], 1);
                }
            }

            if(dic.Count!= 3)
            {
                isFourWithTwo = false;
            }

            //判断是否有四带二的四
            bool hasFour = false;
            foreach(int count in dic.Values)
            {
                if(count == 4)
                {
                    hasFour = true;
                    break;
                }
            }

            if (!hasFour)
            {
                isFourWithTwo = false;
            }

            if (isFourWithTwo)
            {
                cardType = CardType.FOUR_WITH_TWO;
            }
        }

        return cardType;
    }

    /// <summary>
    /// 比较出牌的大小
    /// </summary>
    /// <param name="preCards">上一家出的牌</param>
    /// <param name="curCards">自家准备出的牌</param>
    /// <returns>比上一家出的牌更大，返回true，表示可以出</returns>
    public static bool Compare(Card[] preCards , Card[] curCards)
    {
        /*
         * Array.Sort的第二个参数可以用 IComparer接口 和 Lambda表达式 来自定义排序的方式
         * CompareTo()方法比较两个int参数，结果为负数，说明 (int)card1.rank) 小于 (int)card2.rank
         */
        Array.Sort(preCards, (Card card1, Card card2) => ((int)card1.rank).CompareTo((int)card2.rank));
        Array.Sort(curCards, (Card card1, Card card2) => ((int)card1.rank).CompareTo((int)card2.rank));

        //王炸优先级最高
        if(GetCardType(curCards) == CardType.JOKER_BOMB)
        {
            return true;
        }
        //上家没出炸弹，自家出了炸弹，则炸弹可以出
        if(GetCardType(preCards) != CardType.BOMB && GetCardType(curCards) == CardType.BOMB)
        {
            return true;
        }
        //自家出牌与上家类型一致，开始比较大小
        if(GetCardType(preCards) == GetCardType(curCards))
        {
            switch (GetCardType(curCards))
            {
                case CardType.SINGLE:
                    if (preCards[0].rank < curCards[0].rank)
                        return true;
                    return false;
                case CardType.PAIR:
                    if (preCards[0].rank < curCards[0].rank)
                        return true;
                    return false;
                case CardType.TRIPLE:
                    if (preCards[0].rank < curCards[0].rank)
                        return true;
                    return false;
                case CardType.TRIPLE_WITH_SINGLE:
                    //三带一的话，因为之前排过序，因此下标为1的牌一定是三张中的牌，比较这个即可
                    if (preCards[1].rank < curCards[1].rank)
                        return true;
                    return false;
                case CardType.TRIPLE_WITH_PAIR:
                    //理由同上，三带二比较下标为2的牌即可
                    if (preCards[2].rank < curCards[2].rank)
                        return true;
                    return false;
                case CardType.STRAIGHT:
                    //顺子首先要保证出牌数量一致
                    if (preCards.Length == curCards.Length)
                        //只比较第一个即可
                        if (preCards[0].rank < curCards[0].rank)
                            return true;
                    return false;
                case CardType.STRAIGHT_PAIRS:
                    //连对首先要保证出牌数量一致
                    if (preCards.Length == curCards.Length)
                        //只比较第一个即可
                        if (preCards[0].rank < curCards[0].rank)
                            return true;
                    return false;
                case CardType.AIRPLANE:
                    //飞机首先要保证出牌数量一致
                    if(preCards.Length == curCards.Length)
                        if (preCards[0].rank < curCards[0].rank)
                            return true;
                    return false;
                case CardType.AIRPLANE_WITH_SINGLES:
                    //飞机首先要保证出牌数量一致
                    if (preCards.Length == curCards.Length)
                    {
                        //找到飞机的第一个三张的牌进行比较
                        int preIndex = -1;
                        for(int i =0;i<preCards.Length - 2; i++)
                        {
                            if (preCards[i].rank == preCards[i+1].rank && preCards[i].rank == preCards[i+2].rank)
                            {
                                preIndex = i;
                                break;
                            }
                        }
                        int curIndex = -1;
                        for (int i = 0; i < curCards.Length - 2; i++)
                        {
                            if (curCards[i].rank == curCards[i + 1].rank && curCards[i].rank == curCards[i + 2].rank)
                            {
                                curIndex = i;
                                break;
                            }
                        }

                        if (preCards[preIndex].rank < curCards[curIndex].rank)
                            return true;
                    }
                    return false;
                case CardType.AIRPLANE_WITH_PAIRS:
                    //逻辑与飞机带单翅的一样
                    if (preCards.Length == curCards.Length)
                    {
                        //找到飞机的第一个三张的牌进行比较
                        int preIndex = -1;
                        for (int i = 0; i < preCards.Length - 2; i++)
                        {
                            if (preCards[i].rank == preCards[i + 1].rank && preCards[i].rank == preCards[i + 2].rank)
                            {
                                preIndex = i;
                                break;
                            }
                        }
                        int curIndex = -1;
                        for (int i = 0; i < curCards.Length - 2; i++)
                        {
                            if (curCards[i].rank == curCards[i + 1].rank && curCards[i].rank == curCards[i + 2].rank)
                            {
                                curIndex = i;
                                break;
                            }
                        }

                        if (preCards[preIndex].rank < curCards[curIndex].rank)
                            return true;
                    }
                    return false;
                case CardType.BOMB:
                    if (preCards[0].rank < curCards[0].rank)
                        return true;
                    return false;
                case CardType.JOKER_BOMB:
                case CardType.FOUR_WITH_TWO:
                    //理由同三带二，比较下标为2的牌即可
                    if (preCards[2].rank < curCards[2].rank)
                        return true;
                    return false;
                case CardType.INVALID:
                    break;
            }
        }
        return false;
    }
}

