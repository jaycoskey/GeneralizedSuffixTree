TODO: Convert this README file to github markdown, and put in better location.

Summary and status: Implementation of generalized suffix tree, in progress.
  * Yields incomplete full suffix tree in some cases.
  * Throws NullReferenceException in others.

History of implementation:
  * Started with Mark Nelson's C++ suffix tree implementation: http://marknelson.us/1996/08/01/suffix-trees/
  * Translated to (object-oriented) C#, making it more closely resemble Illya Havsiyevych's Java implemetation.
  * Added plenty of debugging output to better understand the algorithm.
  * Converted to generalized suffix tree, allowing for separate addition of individual words.
      (To the best of my knowledge, no such implementation has been published before, on the web or elsewhere.)

Test cases with published results:
(1) T_0 = "ABAB$" and T_1 = "BABA$".
    See Wikipedia page http://en.wikipedia.org/wiki/Generalised_suffix_tree
	Current results for this test case:
	  NullReferenceException in Canonicalize() function on last character of T_1.

(2) T_0 = "xabxa$", T_1 = "babxba$"
	See course slides by Pekka Kilpeläinen, at http://www.cs.uku.fi/~kilpelai/BSA05/lectures/slides08.pdf
	Current results for this test case:
	  Program creates suffix tree with all suffixes except two for word #1: $ and a$.

(3) T_0 = "cbaab$", T_1 = "baabc^".
    See "Looking for All Palindromes in a String", by Shih Jang Pan and R. C. T. Lee
    Current results for this test case:
      NullReferenceException in Canonicalize() function on last character of T_1.  [See (1), above.]

TODO:
  * Bugfix: Fix exception thrown in test cases (1) and (3), above.
  * Complete: Have program yield correct results for all test cases listed above.
  * Complete: Ensure that program yields correct results for some other simple test cases.  For example:
      - T_0 = "a$", T_1 = "a#"
	  - T_0 = "aaa$"	T_1 = "aaa#"
	  - T_0 = "banana$", T_2 = "bandana$"
	  - etc.
	  - T_0 = "mississippi$", T_1 = "mississippi$"
  * Add feature: Support removal of words from GST.
  * Add feature: Convert into concurrent data structure.
  * Add test: Validate that beginIndexes and endIndexes occur in pairs (or enforce through types).
  * Add test: All index pairs for a given edge represent the same string.
  * Add test: All suffixes present for each word added to suffix tree.
  * Add test: No non-branching internal nodes (before decompression).
  * Add test: Using dictionary, validating results of word pairs, triples, etc.
  * Use for text algorithms:
      - Search for exact expressions occurring in one or more words (w/ info on which words).
	  - Search for approximate expressions occurring in one or more words.
      - Longest repeated substring
      - Longest common substring
      - Longest common palindrome
  * Convert debug print statements to events, to allow listeners that can animate algorithm.