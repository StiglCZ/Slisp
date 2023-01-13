# SLISP
```
    Using default lisp syntax with () replaced by {}
```
## Functions
    ```
    Declaration:
    {funcName}
    ```
    ```
    to create program entrypoint use function main
    Arguments:

    {func
        {{arg1}{arg2}}
    }
    ```
## Hello world
```{main
{var{local}{string}{helloworld}}
{set{string}{helloworld}{"Hello\sworld!"}}
{io{printf}{string}{helloworld}}
}
```
## Variables
```
    Define:
    {var{security}{type}{name}}
    Types: int, string
    Security: local, global
    Name: any
    Assign:
    {set{type}{name}{value}}
```