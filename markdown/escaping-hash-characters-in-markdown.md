# Escaping Hash Characters in Markdown

In the process of creating the root TIL `readme.md`, I wanted to make a header for C# items but was having trouble getting it to appear properly.  In Markdown, you can escape various characters using a backslash (`\`), so if you wanted to type C\#, you would use `C\#`.

Unfortunately, when creating a heading the trailing hash character gets treated as an optional header close tag. 

So `### C\#` turns into: 

### C\#

The easiest way to get around this is by adding a trailing space to the header.

`### C\# ` <-- See it, see the space?

### C\# 

As another note, when you create a header, an anchor is automatically created that you can link to, but any special characters are ignored and it's also all lowercased.  The two headers above both have the same link name due to the ignored special character, so you have to use a numbered link name for anchor.  To link to the C# header above, you would use `[C#](#c-1)` which results in [C#](#c-1).
