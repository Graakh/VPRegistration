CSC = dmcs
VBC = vbnc

NAME = DeathByCaptcha

JSON_SRC = DeathByCaptcha/SimpleJson/*.cs
LIB_SRC = DeathByCaptcha/$(NAME)/*.cs


all: clean lib examples package


clean:
	-rm bin/*.exe bin/*.zip doc/$(NAME).xml lib/$(NAME).dll

.PHONY: lib
lib:
	-mkdir lib doc
	$(CSC) -r:System.Web -target:library -nowarn:1591,1587 -out:lib/$(NAME).dll -doc:doc/$(NAME).xml $(JSON_SRC) $(LIB_SRC)

examples:
	$(MAKE) $(MAKEFLAGS) examples.cs examples.vb

.PHONY: examples.cs
examples.cs:
	-mkdir bin
	$(CSC) -r:lib/$(NAME).dll -target:exe -out:bin/ExampleSimple.cs.exe -main:$(NAME).ExampleSimple DBC_Examples/Examples.cs
	$(CSC) -r:lib/$(NAME).dll -target:exe -out:bin/ExampleAsync.cs.exe -main:$(NAME).ExampleAsync DBC_Examples/Examples.cs
	$(CSC) -r:lib/$(NAME).dll -target:exe -out:bin/ExampleFull.cs.exe -main:$(NAME).ExampleFull DBC_Examples/Examples.cs

.PHONY: examples.vb
examples.vb:
	-mkdir bin
	$(VBC) -r:lib/$(NAME).dll -target:exe -out:bin/ExampleSimple.vb.exe -main:ExampleSimple DBC_Examples_VB/Examples.vb
	$(VBC) -r:lib/$(NAME).dll -target:exe -out:bin/ExampleFull.vb.exe -main:ExampleFull DBC_Examples_VB/Examples.vb

package:
	zip -9rX bin/dbc_api_dotnet.zip doc/$(NAME).xml lib/$(NAME).dll src Makefile Makefile.w32 readme.html -x \*/.svn/\* -x \*.csproj -x src/StressTest.cs
