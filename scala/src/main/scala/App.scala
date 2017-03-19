import common._

sealed abstract class TreeResult[T] {
  val result: T
}

case class Leaf[T](from: Int, to: Int, override val result: T) extends TreeResult[T]

case class Node[T](left: TreeResult[T], override val result: T, right: TreeResult[T]) extends TreeResult[T]


object App {

  val threshold = 2

  //sequential reduce for segment
  def reduceSegment[A](input: Array[A], left: Int, right: Int, a0: A,
                       f: (A, A) => A): A = {
    var a = a0
    var i = left
    while (i < right) {
      a = f(a, input(i))
      i = i + 1
    }

    a
  }


  def scanLeftSequential[A](input: Array[A], left: Int, right: Int, a0: A, f: (A, A) => A,
                            out: Array[A]): Unit = {
    if (left < right) {
      var i = left
      var a = a0
      while (i < right) {
        a = f(a, input(i))
        i = i + 1
        out(i) = a
      }
    }
  }

  def upsweep[T](input: Array[T], from: Int, to: Int, f: (T, T) => T): TreeResult[T] = {
    if (to - from < threshold) {
      Leaf(from, to, reduceSegment(input, from + 1, to, input(from), f))
    } else {
      val mid = from + (to - from) / 2
      val (treeLeft, treeRight) = parallel(
        upsweep(input, from, mid, f),
        upsweep(input, mid, to, f)
      )

      Node(treeLeft, f(treeLeft.result, treeRight.result), treeRight)
    }
  }

  def downsweep[T](input: Array[T], el0: T, f: (T, T) => T, t: TreeResult[T], out: Array[T])
  : Unit = t match {
    case Leaf(from, to, _) => scanLeftSequential(input, from, to, el0, f, out)
    case Node(left, _, right) => {
      val (_, _) = parallel(
        downsweep(input, el0, f, left, out),
        downsweep(input, f(el0, left.result), f, right, out)
      )
    }
  }

  def scanLeft[A](input: Array[A], a0: A, f: (A, A) => A, out: Array[A]): Unit = {
    val t = upsweep(input, 0, input.length, f)
    downsweep(input, a0, f, t, out)

    //prepends a0
    out(0) = a0
  }


  def main(args: Array[String]): Unit = {

    val input = Array(1, 3, 8)
    //> inp  : Array[Int] = Array(1, 3, 8)
    val output = Array(0, 0, 0, 0)

    def f = (s: Int, x: Int) => s + x

    scanLeft(input, 100, f, output)

    output.foreach {
      println
    }
  }
}
