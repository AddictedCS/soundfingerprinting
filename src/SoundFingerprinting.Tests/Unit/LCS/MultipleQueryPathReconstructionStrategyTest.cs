namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class MultipleQueryPathReconstructionStrategyTest
    {
        private const double PermittedGap = 8192d / 5512;
        private readonly IQueryPathReconstructionStrategy multiplePathReconstructionStrategy = new MultipleQueryPathReconstructionStrategy();

        [Test]
        public void ShouldFindLongestIncreasingSequenceWithOneElement()
        {
            var result = multiplePathReconstructionStrategy.GetBestPaths(TestUtilities.GetMatchedWith(new[] { 0 }, new [] { 0 }), PermittedGap).ToList();

            Assert.AreEqual(1, result.Count);
            CollectionAssert.AreEqual(new float[] { 0 }, result[0].Select(with => with.TrackMatchAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence()
        {
            var matches = TestUtilities.GetMatchedWith(
                queryAt: new[] { 0, 1, 2, 10, 11, 12, 13, 14, 15, 16 }, 
                trackAt: new[] { 1, 2, 3, 1,  2,  3,  4,  5,  6,  7 });

            var result = multiplePathReconstructionStrategy.GetBestPaths(matches, PermittedGap).ToList();

            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4, 5, 6, 7 }, result[0].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[1].Select(pair => pair.TrackMatchAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceComplex()
        {
            var matches = TestUtilities.GetMatchedWith(
                new[] { 0, 1, 2,   10, 11, 12, 13,  24, 25, 26 }, 
                new[] { 1, 2, 3,   1,  2,  3,  4,   1, 2, 3 });

            var result = multiplePathReconstructionStrategy.GetBestPaths(matches, 5).ToList();

            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4 }, result[0].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[1].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[2].Select(pair => pair.TrackMatchAt));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceComplex2()
        {
            var matches = TestUtilities.GetMatchedWith(
                new[] { 7, 8, 9, 10, 21, 22, 23, 24, 25, 36, 37, 38 }, 
                new[] { 1, 2, 3, 4,  1,  2,  3,  4,  5,  1,  2,  3 });

            var result = multiplePathReconstructionStrategy.GetBestPaths(matches, 5).ToList();

            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4, 5 }, result[0].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3, 4 }, result[1].Select(pair => pair.TrackMatchAt));
            CollectionAssert.AreEqual(new float[] { 1, 2, 3 }, result[2].Select(pair => pair.TrackMatchAt));
        }

        [Test]
        public void ShouldGrabTheFirstMatch()
        {
            var matches = TestUtilities.GetMatchedWith(new[] {1, 2, 3, 10, 12, 13, 14}, new[] {1, 2, 3, 10, 12, 13, 14});

            var result = multiplePathReconstructionStrategy.GetBestPaths(matches, 5).ToList();
            
            Assert.AreEqual(2, result.Count);
        }
        
         [Test]
        public void ShouldFindLongestIncreasingSequenceEmpty()
        {
            var result = multiplePathReconstructionStrategy.GetBestPaths(Enumerable.Empty<MatchedWith>(), int.MaxValue);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public void ShouldFindLongestIncreasingSequenceTrivial()
        {
            var pairs = new[] {(1, 1)};
            var result = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), int.MaxValue).ToList();

            AssertResult(pairs, result[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence0()
        {
            /*
             * q         1 2 3
             * t         1 2 3
             * expected  x x x
             */
            var pairs = new[] {(1, 1), (2, 2), (3, 3)};
            var result = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), int.MaxValue).First();

            AssertResult(pairs, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence1()
        {
            /*
             * q         1 2 3 4
             * t         1 1 1 2
             * expected  x     x
             * max       1 2 3 4
             */

            var pairs = new[] {(1, 1), (2, 1), (3, 1), (4, 2)};
            var result = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), int.MaxValue).ToList();

            AssertResult(pairs, result[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence2()
        {
            /*
             * q         1 1 1 4
             * t         1 2 3 4
             * expected  x     x
             * max       1 1 1 2
             */

            var pairs = new[] {(1, 1), (1, 2), (1, 3), (4, 4)};
            var result = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), int.MaxValue).First();

            AssertResult(pairs, result);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence3()
        {
            /*
             *
             * q         1 2 3 4
             * t         4 3 2 1
             * expected        x
             * max       1 1 1 1
             * case max' stays the same, t is decreasing
             */

            var pairs = new[] {(1, 4), (2, 3), (3, 2), (4, 1)};
            var result = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), int.MaxValue).ToList();

            Assert.AreEqual(4, result.Count());
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence4()
        {
            /*         ignore reverse
             *               |
             * q         1 2 0 3 4
             * t         1 2 3 3 4
             * expected  x x   x x
             * max       1 2 1 3 4
             * max' is decreasing (by 2), t stays the same 2 elements (need to skip sorting as max indicates we need to skip)
             */

            var pairs = new[] {(1, 1), (2, 2), (0, 3), (3, 3), (4, 4)};
            var result = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), int.MaxValue).ToList();

            var expected = new[] {(1, 1), (2, 2), (3, 3), (4, 4)};

            AssertResult(expected, result[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence5()
        {
            /*
             * q         1 2 3 4 4
             * t         1 2 3 3 4
             * expected  x x x   x
             * max       1 2 3 4 4
             * maxs' stays the same pick by score
             */

            var pairs = new[] {(1, 1), (2, 2), (3, 3), (4, 3), (4, 4)};
            var result = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), int.MaxValue).ToList();

            AssertResult(pairs, result[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence6()
        {
            /*
             * same song appears twice
             * in the query in 2 different locations
             *
             * q          1 20 2 3 21 3 22
             * t          1 1  2 2 2  3 3
             * expected   x y  x   y  x y
             * max        1 2  2 3 4  3 5
             * max (c.)   1 1  2 3 2  3 3
             */

            var pairs = new[] {(1, 1), (20, 1), (2, 2), (3, 2), (21, 2), (3, 3), (22, 3)};
            var results = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), maxGap: 10).ToArray();

            var expected1 = new[] {(1, 1), (2, 2), (3, 3)};
            var expected2 = new[] {(20, 1), (21, 2), (22, 3)};
            var expected3 = new[] { (3, 2) };

            Assert.AreEqual(3, results.Length);
            AssertResult(expected1, results[0]);
            AssertResult(expected2, results[1]);
            AssertResult(expected3, results[2]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence7()
        {
            /*
             * q         1 2 4 3 3
             * t         1 2 3 4 5
             * expected  x x   x x 
             * max       1 2 3 3 3
             */

            var pairs = new[] {(1, 1), (2, 2), (4, 3), (3, 4), (3, 5)};
            var result = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), int.MaxValue).ToList();

            Assert.AreEqual(2, result.Count);
            var expected1 = new[] {(1, 1), (2, 2), (3, 4), (3, 5)};
            var expected2 = new[] { (4, 3) };

            AssertResult(expected1, result[0]);
            AssertResult(expected2, result[1]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence8()
        {
            /*
             * q          20 1 2 21 3 22 4 0 5
             * t          1  1 2 2  3 3  4 4 5
             * expected   y  x x y  x y  x   x
             * max        1  2 2 3  3 4  4 1 5
             * max (c.)   1  1 2 2  3 3  4 1 5
             */

            var pairs = new[] {(20, 1), (1, 1), (2, 2), (21, 2), (3, 3), (22, 3), (4, 4), (0, 4), (5, 5)};

            var results = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), maxGap: 10).ToArray();

            var expected1 = new[] {(1, 1), (2, 2), (3, 3), (4, 4), (5, 5)};
            var expected2 = new[] {(20, 1), (21, 2), (22, 3)};
            var expected3 = new[] { (0, 4) };

            Assert.AreEqual(3, results.Length);
            AssertResult(expected1, results[0]);
            AssertResult(expected2, results[1]);
            AssertResult(expected3, results[2]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence9()
        {
            /*
             * q        1 2 3 4 5  1  2  3  4  5  6
             * t        1 2 3 4 5  20 21 22 23 24 25
             * max (c.) 1 2 3 4 5  1  2  3  4  5  6
             */

            var pairs = new[] {(1, 1), (2, 2), (3, 3), (4, 4), (1, 20), (2, 21), (3, 22), (4, 23), (5, 24), (6, 25)};
            var results = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), maxGap: 6).ToArray();

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

            var pairs = new[] {(1, 1), (2, 2), (3, 3), (4, 4), (5, 5), (6, 6), (0, 20), (2, 21), (3, 22), (4, 23), (5, 24), (7, 25)};
            var results = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), maxGap: 7).ToArray();

            Assert.AreEqual(2, results.Length);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, results[0].Select(_ => (int)_.TrackSequenceNumber));
            CollectionAssert.AreEqual(new[] { 20, 21, 22, 23, 24, 25 }, results[1].Select(_ => (int)_.TrackSequenceNumber));
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence11()
        {
            /*
              * q        1 2 0  1  2
              * t        1 2 20 21 22
              * max (c.) 1 2 1  2  3
              */

            var pairs = new[] {(1, 1), (0, 20), (2, 2)};
            var results = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), maxGap: 5).ToArray();

            var expected1 = new[] {(1, 1), (2, 2)};
            var expected2 = new[] {(0, 20)};
            Assert.AreEqual(2, results.Length);

            AssertResult(expected1, results[0]);
            AssertResult(expected2, results[1]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence12()
        {
            /*
             * q            1 2 2 2
             * t            1 2 3 4
             * max (c.)     1 2 2 2
             */
            var pairs = new[] {(1, 1), (2, 2), (2, 3), (2, 4)};
            var results = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), maxGap: 5).ToArray();

            AssertResult(pairs, results[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence13()
        {
            /*
            * q            1 5 4 3
            * t            1 2 3 4
            * max (c.)     1 2 2 2
            */
            var pairs = new[] {(1, 1), (5, 2), (4, 3), (3, 4)};
            var results = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), maxGap: 5).ToArray();

            var expected1 = new[] {(1, 1), (3, 4)};

            AssertResult(expected1, results[0]);
        }

        [Test]
        public void ShouldFindLongestIncreasingSequence14()
        {
            /*
            * q            1 10 2 11 3 12 1  10 2  11 3  12
            * t            1 1  2 2  3 3  10 10 11 11 12 12
             * max (c.)    1 2  2 3  3 4  1  4  2  5  3  6
            */
            var pairs = new[]
            {
                (1, 1), (10, 1), (2, 2), (11, 2), (3, 3), (12, 3), (1, 10), (10, 10), (2, 11), (11, 11), (3, 12), (12, 12)
            };
            var results = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), maxGap: 12).ToArray();

            var expected1 = new[] {(1, 1), (2, 2), (3, 3), (10, 10), (11, 11), (12, 12)};

            AssertResult(expected1, results[0]);
        }

        /*
         * q         1 2 3 7 8 4 5 6 7 8 2 3 9
         * t         1 2 3 2 3 4 5 6 7 8 7 8 9
         */
        [Test]
        public void ShouldNotIgnoreRepeatingCrossMatches()
        {
            var matchedWiths = new[] { (1, 1), (2, 2), (3, 3), (7, 2), (8, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8), (2, 7), (3, 8), (9, 9) }
                .Select(tuple => new MatchedWith((uint)tuple.Item1, tuple.Item1, (uint)tuple.Item2, tuple.Item2, 0d));

            var bestPaths = multiplePathReconstructionStrategy.GetBestPaths(matchedWiths, int.MaxValue).ToList();
            Assert.AreEqual(3, bestPaths.Count);
            
            var first = bestPaths[0].ToList();
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, first.Select(_ => (int)_.QuerySequenceNumber));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, first.Select(_ => (int)_.TrackSequenceNumber));
           
            var third = bestPaths[1].ToList();
            CollectionAssert.AreEqual(new[] { 2, 3 }, third.Select(_ => (int)_.QuerySequenceNumber)); 
            CollectionAssert.AreEqual(new[] { 7, 8 }, third.Select(_ => (int)_.TrackSequenceNumber));
            
            var second = bestPaths[2].ToList();
            CollectionAssert.AreEqual(new[] { 7, 8 }, second.Select(_ => (int)_.QuerySequenceNumber));
            CollectionAssert.AreEqual(new[] { 2, 3 }, second.Select(_ => (int)_.TrackSequenceNumber));

        }

        [Test]
        public void BruteForceTest()
        {
            var pairs = new[]
            {
                (0, 0), (5, 1), (27, 2), (60, 3), (96, 4), (117, 5), (153, 6), (173, 7), (195, 8), (237, 9),
                (253, 10), (287, 11), (299, 12), (316, 12), (333, 13), (366, 14), (394, 15), (416, 16), (445, 17),
                (504, 19), (10, 21), (36, 22), (51, 23), (70, 23), (88, 24), (125, 25), (144, 26), (179, 27),
                (212, 28), (232, 29), (269, 30), (278, 31), (300, 32), (321, 32), (340, 33), (379, 34), (395, 35),
                (459, 37), (479, 38), (502, 39), (514, 39), (525, 40), (3, 41), (39, 42), (72, 43), (93, 44),
                (105, 44), (126, 45), (140, 46), (201, 48), (234, 49), (255, 50), (275, 51), (294, 51),
                (307, 52), (345, 53), (370, 54), (391, 55), (457, 57), (480, 58), (500, 59), (2, 61), (40, 62),
                (56, 63), (69, 63), (95, 64), (108, 64), (122, 65), (142, 66), (229, 69), (271, 70), (289, 71),
                (309, 72), (369, 74), (400, 75), (420, 76), (461, 77), (478, 78), (501, 79), (538, 80), (1, 81),
                (16, 81), (37, 82), (62, 83), (75, 83), (90, 84), (147, 86), (181, 87), (196, 88), (233, 89),
                (254, 90), (310, 92), (334, 93), (350, 93), (364, 94), (425, 96), (447, 97), (468, 98), (481, 98),
                (497, 99), (511, 99), (530, 100), (9, 101), (31, 102), (74, 103), (97, 104), (118, 105),
                (154, 106), (170, 107), (211, 108), (230, 109), (290, 111), (305, 112), (347, 113), (367, 114),
                (386, 115), (405, 115), (421, 116), (455, 117), (473, 118), (520, 119), (13, 121), (30, 122),
                (44, 122), (64, 123), (129, 125), (148, 126), (183, 127), (203, 128), (264, 130), (282, 131),
                (319, 132), (339, 133), (380, 134), (402, 135), (418, 136), (441, 137), (460, 137), (483, 138),
                (50, 142), (63, 143), (80, 143), (98, 144), (113, 145), (123, 145), (157, 146), (178, 147),
                (239, 149), (257, 150), (277, 151), (292, 151), (317, 152), (353, 153), (378, 154), (401, 155),
                (422, 156), (446, 157), (462, 157), (485, 158), (510, 159), (533, 160), (7, 161), (53, 162),
                (76, 163), (99, 164), (130, 165), (149, 166), (187, 167), (209, 168), (227, 169), (241, 169),
                (259, 170), (286, 171), (308, 172), (327, 172), (344, 173), (403, 175), (427, 176), (453, 177),
                (475, 178), (512, 179), (14, 181), (33, 182), (52, 182), (68, 183), (92, 184), (134, 185),
                (159, 186), (205, 188), (265, 190), (291, 191), (343, 193), (381, 194), (396, 195), (434, 196),
                (449, 197), (487, 198), (507, 199), (526, 200), (18, 201), (41, 202), (59, 203), (78, 203),
                (87, 204), (107, 204), (131, 205), (152, 206), (176, 207), (266, 210), (295, 211), (314, 212),
                (337, 213), (376, 214), (404, 215), (423, 216), (458, 217), (477, 218), (492, 218), (513, 219),
                (24, 221), (42, 222), (66, 223), (109, 224), (127, 225), (145, 226), (216, 228), (238, 229),
                (260, 230), (315, 232), (338, 233), (352, 233), (371, 234), (428, 236), (452, 237), (509, 239),
                (12, 241), (48, 242), (71, 243), (89, 244), (132, 245), (155, 246), (213, 248), (262, 250),
                (322, 252), (341, 253), (372, 254), (398, 255), (408, 255), (424, 256), (464, 257), (482, 258),
                (515, 259), (535, 260), (19, 261), (49, 262), (73, 263), (94, 264), (135, 265), (150, 266),
                (163, 266), (177, 267), (190, 267), (210, 268), (236, 269), (256, 270), (297, 271), (329, 272),
                (351, 273), (377, 274), (407, 275), (430, 276), (519, 279), (539, 280), (29, 281), (43, 282),
                (114, 284), (133, 285), (146, 285), (192, 287), (207, 288), (222, 288), (245, 289), (280, 290),
                (304, 291), (349, 293), (406, 295), (426, 296), (490, 298), (516, 299), (20, 301), (45, 302),
                (106, 304), (121, 305), (141, 305), (156, 306), (188, 307), (219, 308), (252, 309), (273, 310),
                (288, 311), (331, 312), (359, 313), (375, 314), (435, 316), (454, 317), (491, 318), (540, 320),
                (22, 321), (47, 322), (61, 322), (81, 323), (100, 324), (124, 325), (164, 326), (182, 327),
                (199, 327), (221, 328), (244, 329), (303, 331), (325, 332), (348, 333), (389, 334), (414, 335),
                (438, 336), (463, 337), (489, 338), (505, 338), (521, 339), (541, 340), (17, 341), (58, 342),
                (82, 343), (101, 344), (120, 345), (139, 345), (158, 346), (180, 347), (197, 347), (208, 348),
                (246, 349), (301, 351), (323, 352), (357, 353), (373, 354), (399, 355), (417, 355), (444, 356),
                (456, 357), (486, 358), (506, 359), (54, 362), (77, 363), (104, 364), (138, 365), (162, 366),
                (185, 367), (228, 368), (248, 369), (270, 370), (293, 371), (320, 372), (336, 372), (354, 373),
                (390, 374), (410, 375), (466, 377), (488, 378), (524, 379)
            };

            var results = multiplePathReconstructionStrategy.GetBestPaths(Generate(pairs), maxGap: 600).ToArray();
            var coverages = results.Select(_ => new Coverage(_, 600d, 600d, 1.48d, permittedGap: 600));
            var best = OverlappingRegionFilter.FilterCrossMatchedCoverages(coverages);
            // this is debatable, but I don't have a good solution for cases when only track coverage is contained between 2 coverages
            Assert.AreEqual(2, best.Count());
            
            var matchedWiths = results.First().ToList();
            var noSideEffects = multiplePathReconstructionStrategy.GetBestPaths(matchedWiths, int.MaxValue).First().ToList();
            
            Assert.AreEqual(matchedWiths.Count, noSideEffects.Count);
            foreach (var pair in matchedWiths.Zip(noSideEffects, (a, b) => (a, b)))
            {
                Assert.AreEqual(pair.a.QueryMatchAt, pair.b.QueryMatchAt);
                Assert.AreEqual(pair.a.TrackMatchAt, pair.b.TrackMatchAt);
                Assert.AreEqual(pair.a.QuerySequenceNumber, pair.b.QuerySequenceNumber);
                Assert.AreEqual(pair.a.TrackSequenceNumber, pair.b.TrackSequenceNumber);
            }
        }

        private static void AssertResult((int q, int t)[] pairs, IEnumerable<MatchedWith> result)
        {
            var matches = result as MatchedWith[] ?? result.ToArray();
            CollectionAssert.AreEqual(pairs.Select(_ => _.q), matches.Select(_ => _.QuerySequenceNumber));
            CollectionAssert.AreEqual(pairs.Select(_ => _.t), matches.Select(_ => _.TrackSequenceNumber));
        }

        private static IEnumerable<MatchedWith> Generate((int q, int t)[] queryTrackPairs)
        {
            return queryTrackPairs.Select(pair =>
            {
                (int q, int t) = pair;
                return new MatchedWith((uint) q, q, (uint) t, t, 0d);
            });
        }
    }
}
