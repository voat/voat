# Voat Branch for .NET Core Port (With the PostgreSQL Branch Merged)

Currently using .NET Core 2 Preview



Phase #1
Get it compiling. 

## Standards on Phase #1
If there is not a direct path to update code to .NET Core 2 Preview, comment it out using these standards:

For sections of code inside a method:

~~~
//CORE_PORT: <Reason>
throw new NotImplementedException("Core Port");
/*
  -- what you commented out
*/
~~~


For entire methods:
~~~
//CORE_PORT: <Reason>
/*  
  public void SomeMethod(...)
  ...
*/
~~~

For single line issues such as attributes where you can not throw an exception:
~~~
//CORE_PORT: <Reason>
// This is what was commented out 
~~~

... more to come
