using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ExitGames.Logging;

namespace PhotonIntro
{
    public class ReusableList<T> : List<T>
    {
        private readonly ILogger log = LogManager.GetCurrentClassLogger();

        private ConcurrentStack<int> reuseStack;
        private double cap;
        public int solidCount;
        public int listLength
        {
            get
            {
                return this.Count;
            }
        }


        public ReusableList(int iniC, double maxNum = Double.PositiveInfinity)
            : base(iniC)
        {
            reuseStack = new ConcurrentStack<int>();
            cap = maxNum;
            solidCount = 0;
        }

        public int Add(T newOne, int index) { 
            if (index >= this.listLength){
                log.Error("YOU ARE REPLACING A CELL INDEX OUT OF BOUND OF THE LIST");
                return -1;
            }

            this[index] = newOne;
            this.solidCount += 1;

            return index;
        }

        new public int Add(T newOne)
        {
            //check if it's over the cap
            if (this.solidCount >= cap)
            {
                return -1;
            }

            //increment
            this.solidCount += 1;

            // if reuse stack has a pos, use it
            // else append to a new pos
            int ID;

            while (true)
            {
                //nothing in there. break and append
                if (reuseStack.Count == 0) { break; }

                if (reuseStack.TryPop(out ID))
                {
                    this[ID] = newOne;
                    return ID;
                }
            }

            //appending to the end
            ID = this.Count;
            base.Add(newOne);

            return ID;
        }


        internal void remove(int index, bool reusing = true)
        {
            //drop and push the index to stack
            this[index] = default(T);
            if (reusing)
            {
                reuseStack.Push(index);
            }

            //decrease
            solidCount -= 1;

        }

        internal void changeCap(int newCap)
        {
            cap = newCap;
        }
    }
}