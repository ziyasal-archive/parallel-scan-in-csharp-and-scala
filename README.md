# parallel scan in C# & Scala
```sh
# 1.    
      /\              
     /  \ 
    /\   /\
   1  3  8 50

# 2.
     62
     /\
    4  58
   /\   /\
  1  3  8 50
  
# 3. Perform scan left for number 100:
            62
            / \
           /   \
          /     \
         /       \
        /         \
 (100) 4           58 (104)
      / \           /\ 
101  /   \104   112/  \ 162
    1     3       8     50
    
# 4.  

[ 100, 101, 104, 112, 162]
```

ziyasal
@MIT
