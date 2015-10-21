# Cecil Prototype
Prototype where a given assembly is instrumentated with the mono.cecil framework.

Following IL-OpCodes are instrumentated:

-	ldsfld		(load static field)
-	initobj		(write of a value type)
-	stsfld		(store static field)
-	ldfld		(load field)
-	stfld 		(store field)
-	ldelem		(load element of array)
-	stelem		(store element of array)
-	call		(call a method, for Monitor.Enter() and Monitor.Exit())
-	callvirt 	(load element of a arraylist)
