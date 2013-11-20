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
		%out = %this.numToAlpha((%char + getField(%this.OTPNums, 0)) % 26); // remove nums after use, get the first number
		%outputStr = %outputStr @ %out;
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
		// %out = %this.numToAlpha((%char + getField(%this.OTPNums, 0)) % 26); // remove nums after use, get the first number // DECRYPTION IS JUST REVERSE, YES? CHECK.
		%outputStr = %outputStr @ %out;
	}

	return %outputStr;
}

function DiffieHellman::alphaToNum(%this, %char)
{
	
}

function DiffieHellman::numToAlpha(%this, %char)
{

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

	%squareRoot = Math_SquareRoot(%num); // oops we don't have one of these...

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