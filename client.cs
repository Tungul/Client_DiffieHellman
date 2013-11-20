exec("./src/diffie.cs");
exec("./lib/rand.cs");
// exec("./res/primes.cs"); // requires converting prime.txt, but probably faster

// import prime numbers
%primelister = new FileObject();
%primelister.openForRead("./res/primes.txt");
%i = 0;
while(!%primelister.isEOF())
{
	$PrimeList[%i] = %primelister.readLine();
}
%primelister.close(); %primelister.delete();