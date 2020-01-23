namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class LISTest
    {
        [Test]
        public void ShouldFindLongestIncreasingSequenceEmpty()
        {
            var result = LisNew.GetIncreasingSequences(Enumerable.Empty<MatchedWith>());
            
            Assert.IsFalse(result.Any());
        }
        
        [Test]
        public void ShouldFindLongestIncreasingSequenceTrivial()
        {
            var pairs = new[] {(1, 1, 0d)};
            var result = LisNew.GetIncreasingSequences(Generate(pairs)).First();
            
            AssertResult(pairs, result);
        }
        
        [Test]
        public void ShouldFindLongestIncreasingSequence0()
        {
            /*
             * q         1 2 3 
             * t         1 2 3
             * expected  x x x
             */
            var pairs = new[] {(1, 1, 0d), (2, 2, 0d), (3, 3, 0d)};
            var result = LisNew.GetIncreasingSequences(Generate(pairs)).First();

            AssertResult(pairs, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence1()
        {
            /* s         4 3 2 1   
             * q         1 2 3 4
             * t         1 1 1 2    
             * expected  x     x
             * max       1 2 3 4
             * case max' is decreasing, t stays the same last 3 elements (need to pick only one)
             */

            var pairs = new[] {(1, 1, 4d), (2, 1, 3), (3, 1, 2), (4, 2, 1)};
            var result = LisNew.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(1, 1, 4d), (4, 2, 1)};
            AssertResult(expected, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence2()
        {
            /*
             *      pick best score
             *           |
             * score     3 2 1 1 
             * q         1 1 1 4
             * t         1 2 3 4
             * expected  x     x
             * max       1 1 1 2
             * case max' stays the same last 3 elements, t' is decreasing (need to pick only one by score)
             */

            var pairs = new[] {(1, 1, 3d), (1, 2, 2), (1, 3, 1), (4, 4, 1)};
            var result = LisNew.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(1, 1, 3), (4, 4, 1d)};
            AssertResult(expected, result);
        }


        [Test]
        public void ShouldFindLongestIncreasingSequence3()
        {
            /* reversed pick best score
             * 
             * score     0 1 2 3
             * q         1 2 3 4
             * t         4 3 2 1
             * expected        x
             * max       1 1 1 1
             * case max' stays the same, t is decreasing, (need to pick only one by score)
             */

            var pairs = new[] {(1, 4, 0d), (2, 3, 1), (3, 2, 2), (4, 1, 3)};
            var result = LisNew.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(4, 1, 3d)};

            AssertResult(expected, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence4()
        {
            /*         ignore reverse   
             *               |
             * score     1 1 2 1 1
             * q         1 2 0 3 4
             * t         1 2 3 3 4
             * expected  x x   x x
             * max       1 2 1 3 4
             * max' is decreasing (by 2), t stays the same 2 elements (need to skip sorting as max indicates we need to skip)
             */

            var pairs = new[] {(1, 1, 1d), (2, 2, 1), (0, 3, 2), (3, 3, 1), (4, 4, 1)};
            var result = LisNew.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(1, 1, 1d), (2, 2, 1), (3, 3, 1), (4, 4, 1)};

            AssertResult(expected, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence5()
        {
            /*
             *         pick best score
             *               |
             * score     1 1 2 1 1
             * q         1 2 3 4 4
             * t         1 2 3 3 4
             * expected  x x x   x
             * max       1 2 3 4 4
             * maxs' stays the same pick by score
             */

            var pairs = new[] {(1, 1, 1d), (2, 2, 1), (3, 3, 2), (4, 3, 1), (4, 4, 1)};
            var result = LisNew.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(1, 1, 1d), (2, 2, 1), (3, 3, 2), (4, 4, 1)};

            AssertResult(expected, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence6()
        {
            /*
             * same song appears twice
             * in the query in 2 different locations
             *
             *   pick best scoring option for first encounter
             *                 |
             * s          0  0 1 0 0  0 1
             * q          1 20 2 3 21 3 22  
             * t          1 1  2 2 2  3 3
             * expected   x y  x   y  x y
             * max        1 2  2 3 4  3 5
             * max (c.)   1 1  2 3 2  3 3
             */

            var pairs = new[] {(1, 1, 0d), (20, 1, 0), (2, 2, 1), (3, 2, 0), (21, 2, 0), (3, 3, 0), (22, 3, 1)};
            var results = LisNew.GetIncreasingSequences(Generate(pairs), 10).ToArray();

            var expected1 = new[] {(1, 1, 0d), (2, 2, 1), (3, 3, 0)};
            var expected2 = new[] {(20, 1, 0d), (21, 2, 0), (22, 3, 1)};

            Assert.AreEqual(2, results.Length);
            AssertResult(expected1, results[0]);
            AssertResult(expected2, results[1]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence7()
        {
            /* reversed pick best score
             * s         0 0 2 1 0
             * q         1 2 4 3 3
             * t         1 2 3 4 5
             * expected  x x x
             * max       1 2 3 3 3
             * maxs' stays the same pick only one (by score)
             */

            var pairs = new[] {(1, 1, 0d), (2, 2, 0), (4, 3, 2), (3, 4, 1), (3, 5, 0)};
            var result = LisNew.GetIncreasingSequences(Generate(pairs)).First();

            var expected = new[] {(1, 1, 0d), (2, 2, 0), (4, 3, 2)};

            AssertResult(expected, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence8()
        {
            /*
             * s
             * q          20 1 2 21 3 22 4 0 5 
             * t          1  1 2 2  3 3  4 4 5 
             * expected   y  x x y  x y  x   x
             * max        1  2 2 3  3 4  4 1 5
             * max (c.)   1  1 2 2  3 3  4 1 5
             */

            var pairs = new[] {(20, 1, 0d), (1, 1, 0), (2, 2, 0), (21, 2, 0), (3, 3, 0), (22, 3, 1), (4, 4, 0), (0, 4, 0), (5, 5, 0)};

            var results = LisNew.GetIncreasingSequences(Generate(pairs), 10).ToArray();

            var expected1 = new[] {(1, 1, 0d), (2, 2, 0), (3, 3, 0), (4, 4, 0), (5, 5, 0)};
            var expected2 = new[] {(20, 1, 0), (21, 2, 0), (22, 3, 1d)};

            Assert.AreEqual(2, results.Length);
            AssertResult(expected1, results[0]);
            AssertResult(expected2, results[1]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence9()
        {
            /*
             * q        1 2 3 4 5  1  2  3  4  5  6
             * t        1 2 3 4 5  20 21 22 23 24 25
             * max (c.) 1 2 3 4 5  1  2  3  4  5  6
             */

            var pairs = new[] {(1, 1, 0d), (2, 2, 0), (3, 3, 0), (4, 4, 0), (1, 20, 0), (2, 21, 0), (3, 22, 0), (4, 23, 0), (5, 24, 0), (6, 25, 0)};
            var results = LisNew.GetIncreasingSequences(Generate(pairs), 6).ToArray();
            
            Assert.AreEqual(2, results.Length);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence10()
        {
            /*
              * q        1 2 3 4 5 6 0  2  3  4  5  7
              * t        1 2 3 4 5 6 20 21 22 23 24 25
              * max (c.) 1 2 3 4 5 6 1  2  3  4  5  7
              */ 
            
            var pairs = new[] {(1, 1, 0d), (2, 2, 0), (3, 3, 0), (4, 4, 0), (5, 5, 0), (6, 6, 0), (0, 20, 0), (2, 21, 0), (3, 22, 0), (4, 23, 0), (5, 24, 0), (7, 25, 0)};
            var results = LisNew.GetIncreasingSequences(Generate(pairs), 7).ToArray();
            
            Assert.AreEqual(2, results.Length);
        }
        
        [Test]
        public void ShouldFindLongestIncreasingSequence11()
        {
            /*
              * q        1 2 0  1  2
              * t        1 2 20 21 22  
              * max (c.) 1 2 1  2  3
              */

            var pairs = new[] {(1, 1, 0d), (0, 20, 0), (2, 2, 0)};
            var results = LisNew.GetIncreasingSequences(Generate(pairs), 5).ToArray();

            var expected1 = new[] {(1, 1, 0d), (2, 2, 0)};
            var expected2 = new[] {(0, 20, 0d)};
            Assert.AreEqual(2, results.Length);
            
            AssertResult(expected1, results[0]);
            AssertResult(expected2, results[1]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence12()
        {
            /*
             * s            0 1 2 3
             * q            1 2 2 2
             * t            1 2 3 4
             * max (c.)     1 2 2 2
             */
            var pairs = new[] {(1, 1, 0d), (2, 2, 1), (2, 3, 2), (2, 4, 3)};
            var results = LisNew.GetIncreasingSequences(Generate(pairs), 5).ToArray();

            var expected1 = new[] {(1, 1, 0d), (2, 4, 3)};
            
            AssertResult(expected1, results[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence13()
        {
            /*
            * s
            * q            1 5 4 3
            * t            1 2 3 4
            * max (c.)     1 2 2 2
            */
            var pairs = new[] {(1, 1, 0d), (5, 2, 0), (4, 3, 0), (3, 4, 0)};
            var results = LisNew.GetIncreasingSequences(Generate(pairs), 5).ToArray();

            var expected1 = new[] {(1, 1, 0d), (3, 4, 0)};
            
            AssertResult(expected1, results[0]);
        }
        
        [Test]
        public void ShouldFindLongestIncreasingSequence14()
        {
            /*
            * s            0 0  0 1  0 0  0  0  0  0  0  0
            * q            1 10 2 11 3 12 1  10 2  11 3  12  
            * t            1 1  2 2  3 3  10 10 11 11 12 12
             * max (c.)    1 2  2 3  3 4  1  4  2  5  3  6 
            */
            var pairs = new[]
            {
                (1, 1, 0d), (10, 1, 0), (2, 2, 0), (11, 2, 1),
                (3, 3, 0), (12, 3, 0), (1, 10, 0), (10, 10, 0), (2, 11, 0), (11, 11, 0), (3, 12, 0), (12, 12, 0)
            };
            var results = LisNew.GetIncreasingSequences(Generate(pairs), 12).ToArray();

            var expected1 = new[] {(1, 1, 0d), (2, 2, 0), (3, 3, 0), (10, 10, 0), (11, 11, 0), (12, 12, 0)};
            
            AssertResult(expected1, results[0]);
        }

        [Test]
        public void BruteForceTest()
        {
            var pairs = new[]
            {
                (0, 0, 100d), (5, 1, 82), (27, 2, 62), (60, 3, 72), (96, 4, 93), (117, 5, 97), (153, 6, 76), (173, 7, 98), (195, 8, 71), (237, 9, 77),
                (253, 10, 77), (287, 11, 92), (299, 12, 61), (316, 12, 76), (333, 13, 79), (366, 14, 90), (394, 15, 90), (416, 16, 73), (445, 17, 94),
                (504, 19, 95), (10, 21, 97), (36, 22, 90), (51, 23, 53), (70, 23, 68), (88, 24, 94), (125, 25, 72), (144, 26, 78), (179, 27, 85),
                (212, 28, 62), (232, 29, 86), (269, 30, 66), (278, 31, 70), (300, 32, 56), (321, 32, 67), (340, 33, 89), (379, 34, 64), (395, 35, 97),
                (459, 37, 65), (479, 38, 92), (502, 39, 87), (514, 39, 71), (525, 40, 79), (3, 41, 71), (39, 42, 84), (72, 43, 75), (93, 44, 99),
                (105, 44, 64), (126, 45, 78), (140, 46, 68), (201, 48, 87), (234, 49, 86), (255, 50, 100), (275, 51, 67), (294, 51, 63),
                (307, 52, 84), (345, 53, 67), (370, 54, 90), (391, 55, 88), (457, 57, 72), (480, 58, 85), (500, 59, 69), (2, 61, 70), (40, 62, 94),
                (56, 63, 68), (69, 63, 83), (95, 64, 98), (108, 64, 58), (122, 65, 81), (142, 66, 58), (229, 69, 88), (271, 70, 55), (289, 71, 87),
                (309, 72, 74), (369, 74, 99), (400, 75, 83), (420, 76, 77), (461, 77, 57), (478, 78, 94), (501, 79, 62), (538, 80, 81), (1, 81, 60),
                (16, 81, 79), (37, 82, 96), (62, 83, 84), (75, 83, 67), (90, 84, 84), (147, 86, 86), (181, 87, 81), (196, 88, 65), (233, 89, 97),
                (254, 90, 76), (310, 92, 88), (334, 93, 71), (350, 93, 69), (364, 94, 72), (425, 96, 97), (447, 97, 84), (468, 98, 59), (481, 98, 83),
                (497, 99, 64), (511, 99, 82), (530, 100, 91), (9, 101, 78), (31, 102, 62), (74, 103, 72), (97, 104, 96), (118, 105, 70),
                (154, 106, 96), (170, 107, 66), (211, 108, 69), (230, 109, 75), (290, 111, 90), (305, 112, 67), (347, 113, 79), (367, 114, 92),
                (386, 115, 62), (405, 115, 75), (421, 116, 80), (455, 117, 90), (473, 118, 86), (520, 119, 73), (13, 121, 100), (30, 122, 73),
                (44, 122, 81), (64, 123, 78), (129, 125, 75), (148, 126, 88), (183, 127, 67), (203, 128, 81), (264, 130, 82), (282, 131, 62),
                (319, 132, 78), (339, 133, 76), (380, 134, 67), (402, 135, 85), (418, 136, 75), (441, 137, 57), (460, 137, 68), (483, 138, 86),
                (50, 142, 60), (63, 143, 76), (80, 143, 72), (98, 144, 97), (113, 145, 59), (123, 145, 90), (157, 146, 78), (178, 147, 85),
                (239, 149, 81), (257, 150, 86), (277, 151, 64), (292, 151, 90), (317, 152, 97), (353, 153, 66), (378, 154, 77), (401, 155, 100),
                (422, 156, 65), (446, 157, 75), (462, 157, 68), (485, 158, 84), (510, 159, 98), (533, 160, 90), (7, 161, 73), (53, 162, 68),
                (76, 163, 67), (99, 164, 95), (130, 165, 72), (149, 166, 80), (187, 167, 62), (209, 168, 95), (227, 169, 74), (241, 169, 70),
                (259, 170, 89), (286, 171, 82), (308, 172, 67), (327, 172, 60), (344, 173, 94), (403, 175, 88), (427, 176, 96), (453, 177, 93),
                (475, 178, 80), (512, 179, 91), (14, 181, 80), (33, 182, 59), (52, 182, 72), (68, 183, 91), (92, 184, 68), (134, 185, 72),
                (159, 186, 77), (205, 188, 90), (265, 190, 95), (291, 191, 93), (343, 193, 96), (381, 194, 81), (396, 195, 68), (434, 196, 65),
                (449, 197, 70), (487, 198, 82), (507, 199, 91), (526, 200, 61), (18, 201, 95), (41, 202, 87), (59, 203, 50), (78, 203, 80),
                (87, 204, 61), (107, 204, 63), (131, 205, 84), (152, 206, 89), (176, 207, 67), (266, 210, 99), (295, 211, 83), (314, 212, 84),
                (337, 213, 74), (376, 214, 97), (404, 215, 100), (423, 216, 72), (458, 217, 97), (477, 218, 76), (492, 218, 59), (513, 219, 92),
                (24, 221, 63), (42, 222, 84), (66, 223, 74), (109, 224, 69), (127, 225, 99), (145, 226, 64), (216, 228, 74), (238, 229, 100),
                (260, 230, 88), (315, 232, 90), (338, 233, 59), (352, 233, 78), (371, 234, 86), (428, 236, 86), (452, 237, 73), (509, 239, 90),
                (12, 241, 68), (48, 242, 98), (71, 243, 86), (89, 244, 69), (132, 245, 87), (155, 246, 79), (213, 248, 98), (262, 250, 83),
                (322, 252, 95), (341, 253, 66), (372, 254, 94), (398, 255, 73), (408, 255, 63), (424, 256, 74), (464, 257, 83), (482, 258, 86),
                (515, 259, 97), (535, 260, 74), (19, 261, 86), (49, 262, 88), (73, 263, 88), (94, 264, 68), (135, 265, 82), (150, 266, 81),
                (163, 266, 81), (177, 267, 65), (190, 267, 77), (210, 268, 93), (236, 269, 82), (256, 270, 73), (297, 271, 86), (329, 272, 64),
                (351, 273, 87), (377, 274, 97), (407, 275, 78), (430, 276, 91), (519, 279, 92), (539, 280, 94), (29, 281, 68), (43, 282, 75),
                (114, 284, 75), (133, 285, 93), (146, 285, 58), (192, 287, 80), (207, 288, 67), (222, 288, 70), (245, 289, 69), (280, 290, 62),
                (304, 291, 74), (349, 293, 92), (406, 295, 86), (426, 296, 64), (490, 298, 86), (516, 299, 79), (20, 301, 91), (45, 302, 80),
                (106, 304, 94), (121, 305, 71), (141, 305, 58), (156, 306, 77), (188, 307, 98), (219, 308, 82), (252, 309, 62), (273, 310, 71),
                (288, 311, 65), (331, 312, 71), (359, 313, 73), (375, 314, 76), (435, 316, 95), (454, 317, 62), (491, 318, 93), (540, 320, 91),
                (22, 321, 95), (47, 322, 82), (61, 322, 66), (81, 323, 92), (100, 324, 80), (124, 325, 70), (164, 326, 96), (182, 327, 82),
                (199, 327, 61), (221, 328, 75), (244, 329, 88), (303, 331, 70), (325, 332, 87), (348, 333, 88), (389, 334, 64), (414, 335, 70),
                (438, 336, 93), (463, 337, 95), (489, 338, 99), (505, 338, 59), (521, 339, 100), (541, 340, 95), (17, 341, 66), (58, 342, 70),
                (82, 343, 87), (101, 344, 88), (120, 345, 57), (139, 345, 80), (158, 346, 78), (180, 347, 56), (197, 347, 77), (208, 348, 71),
                (246, 349, 92), (301, 351, 90), (323, 352, 98), (357, 353, 88), (373, 354, 74), (399, 355, 61), (417, 355, 63), (444, 356, 71),
                (456, 357, 67), (486, 358, 79), (506, 359, 68), (54, 362, 87), (77, 363, 84), (104, 364, 94), (138, 365, 86), (162, 366, 95),
                (185, 367, 78), (228, 368, 62), (248, 369, 90), (270, 370, 98), (293, 371, 80), (320, 372, 61), (336, 372, 65), (354, 373, 98),
                (390, 374, 59), (410, 375, 83), (466, 377, 95), (488, 378, 74), (524, 379, 87)
            };
            
            var results = LisNew.GetIncreasingSequences(Generate(pairs), 1000).ToArray();
            
            Assert.AreEqual(1, results.Length);
        }

        private static void AssertResult((int q, int t, double s)[] pairs, IEnumerable<MatchedWith> result)
        {
            var matches = result as MatchedWith[] ?? result.ToArray();
            CollectionAssert.AreEqual(pairs.Select(_ => _.q), matches.Select(_ => _.QuerySequenceNumber));
            CollectionAssert.AreEqual(pairs.Select(_ => _.t), matches.Select(_ => _.TrackSequenceNumber));
            CollectionAssert.AreEqual(pairs.Select(_ => _.s), matches.Select(_ => _.Score));
        }

        private static IEnumerable<MatchedWith> Generate((int q, int t, double s)[] queryTrackPairs)
        {
            return queryTrackPairs.Select(pair =>
            {
                (int q, int t, double s) = pair;
                return new MatchedWith((uint) q, q, (uint) t, t, s);
            });
        }
    }
}