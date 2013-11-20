exec("./lib/Math.cs");
exec("./src/prime.cs");
exec("./lib/rand.cs")

function Math_fastExpMod(%x, %e, %m) // should move this to ./lib/math.cs
{
	%y = "1";

	%z = %x;

	while(aLessThanB("0", %e))
	{
		if(Math_Mod(%e ,"2") !$= "0")
		{
			%y = Math_Mod(Math_Multiply(%y, %z), %m);
		}

		%z = Math_Mod(Math_Multiply(%z, %z), %m);

		%e = Math_DivideFloor(%e, 2);
	}

	return %y;
}

new ScriptObject(DiffieHellman);

function DiffieHellman::generateDiffie(%this, %prime, %base)
{
	// need a better random number function
	%context = createRandContext();
	// like i'm not even kidding, this thing will literally not work worth shit (no offense port)
	if(%prime !$= "" || %base !$= "")
	{
		%this.prime = rand(%context, 1111111, 999999, 0);
		while(!Math_isPrime(%this.prime))
		{
			%this.prime = rand(%context, 1111111, 999999, 0); //I get the feeling this won't work right.
		}

		%this.base = rand(%context, 1111111, 999999, 0);
		echo("Public prime: " @ %this.prime);
		echo("Public base: " @ %this.base);
	}
	%this.Private = rand(%context, 222222, 999999, 0);
	%this.PublicKey = Math_fastExpMod(%this.base, %this.Private, %this.prime);
	echo("Public Key: " @ %this.PublicKey);
	return %this.PublicKey;
}

function DiffieHellman::finalizeDiffieHandshake(%this, %bobspublic)
{
	%this.Secret = Math_fastExpMod(%bobspublic, %this.Private, %this.prime);
	%this.randomContext = createRandContext(%this.secret); // needs a constant context for one time pad generation

	return %this.Secret;
}

function DiffieHellman::getOTPNums(%this, %num) // generates an arbitrary number of one time pad numbers
{
	for(%i = 0; %i < %num; %i++)
	{
		%this.OTPNums = %this.OTPNums @ rand(%this.randomContext, 1, 25, 0) @ "\t"; // 1 and 26?
	}
}

function DiffieHellman::getOTPNum(%this)
{
	%num = getField(%this.OTPNums, 0);

	%this.OTPNums = removeField(%this.OTPNums, 0);

	return %num;
}

function DiffieHellman::encrypt(%this, %str)
{
	for(%i = 0; %i < strLen(%str); %i++)
	{
		%char = getSubStr(%str, %i, 0); // or 1?
		if(%char $= "")
		{
			%outputStr = %outputStr @ " "; // can we += here?
			continue;
		}
		%char = %this.alphaToNum(%char); // convert it to a numeral somehow. NOTE: only alphabetical characters. if it isn't alphabetical, strip it or make our OTP work with a large charset

		%out = (%char + %this.getOTPNum()); // remove nums after use, get the first number

		if(%out > 26) // ALTERNATELY USE XOR
			%out -= 26;

		%outputStr = %outputStr @ %this.numToAlpha(%out);
	}

	return %outputStr;
}

function DiffieHellman::decrypt(%this, %str)
{
	for(%i = 0; %i < strLen(%str); %i++)
	{
		%char = getSubStr(%str, %i, 0); // or 1?
		if(%char $= "")
		{
			%outputStr = %outputStr @ " "; // can we += here?
			continue;
		}
		%char = %this.alphaToNum(%char); // convert it to a numeral somehow. NOTE: only alphabetical characters. if it isn't alphabetical, strip it or make our OTP work with a large charset

		%out = (%char - %this.getOTPNum()); // remove nums after use, get the first number

		if(%out < 0) // ALTERNATELY USE XOR
			%out += 26;

		%outputStr = %outputStr @ %this.numToAlpha(%out);
	}

	return %outputStr;
}

function DiffieHellman::alphaToNum(%this, %char)
{
	%alphabet = "a b c d e f g h i j k l m n o p q r s t u v w x y z";
	%numbers = "1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26";

	for(%i = 0; %i < 26; %i++)
	{
		if(%char $= getWord(%alphabet, %i))
		{
			return %i;
		}
	}
}

function DiffieHellman::numToAlpha(%this, %char)
{
	%alphabet = "a b c d e f g h i j k l m n o p q r s t u v w x y z";

	return getWord(%alphabet, %char); // shockingly simple
}

function Math_isPrime(%num) // this is the basic function, it needs to be implemented utilizing APA (Abritrary Precision Arithmetic).
{
	if(%num $= "1" || %num $= "2" || %num $= "3") //Happy, Xalos?
		return true;

	if(Math_Mod(%num, 2) $= "0")
		return false;

	if(Math_Mod(Math_Add(%num, 1), 6) $= "0" || Math_Mod(Math_Subtact(num, 1), 6) $= "0")
		continue;
	else
		return false; // holy shit this is so fucking inefficient and probably broken. i totally screwed something up here.

	// %squareRoot = Math_SquareRoot(%num); // oops we don't have one of these...

	%squareRoot = Math_DivideFloor(%num, 5); // best I can come up with.

	for(%i = "3"; aLessThanB(%i, %squareRoot); %i = Math_Add(%i, 2)) // this can be sped up if we generate a set of primes using the sieve of eratsones, or just flat out include a list of primes from like, one to a million.
	{
		if(Math_Mod(%num, %i) $= "0")
		{
			return false;
		}
	}

	return true;
}

// TODO: "Math_SquareRoot()" or equivalent