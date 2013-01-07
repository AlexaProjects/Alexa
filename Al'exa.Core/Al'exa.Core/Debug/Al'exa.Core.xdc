<?xml version="1.0"?><doc>
<members>
<member name="M:cvSetIdentity(System.Void*,CvScalar)" decl="true" source="c:\opencv-242\build\include\opencv2\core\core_c.h" line="766">
* Finds selected eigen values and vectors of a symmetric matrix */
</member>
<!-- Discarding badly formed XML document comment for member 'M:cv.op_LeftShift(std.basic_ostream<System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte,std.char_traits{System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte}>*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,cv.Mat!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)'. -->
<!-- Discarding badly formed XML document comment for member 'M:cv.op_LeftShift(std.basic_ostream<System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte,std.char_traits{System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte}>*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,cv.Formatted!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)'. -->
<member name="M:cv.FREAK.#ctor(System.Boolean,System.Boolean,System.Single,System.Int32,std.vector&lt;System.Int32&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="true" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="319">
Constructor
         * @param orientationNormalized enable orientation normalization
         * @param scaleNormalized enable scale normalization
         * @param patternScale scaling of the description pattern
         * @param nbOctave number of octaves covered by the detected keypoints
         * @param selectedPairs (optional) user defined selected pairs

</member>
<member name="M:cv.FREAK.descriptorSize" decl="true" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="336">
returns the descriptor length in bytes 
</member>
<member name="M:cv.FREAK.descriptorType" decl="true" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="339">
returns the descriptor type 
</member>
<member name="M:cv.FREAK.selectPairs(std.vector&lt;cv.Mat&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,std.vector&lt;std.vector&lt;cv.KeyPoint&gt;&gt;*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,System.Double!System.Runtime.CompilerServices.IsConst,System.Boolean)" decl="true" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="342">
select the 512 "best description pairs"
         * @param images grayscale images set
         * @param keypoints set of detected keypoints
         * @param corrThresh correlation threshold
         * @param verbose print construction information
         * @return list of best pair indexes

</member>
<member name="T:cv.AdjusterAdapter" decl="false" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="638">
\brief A feature detector parameter adjuster, this is used by the DynamicAdaptedFeatureDetector
 *  and is a wrapper for FeatureDetector that allow them to be adjusted after a detection

</member>
<member name="M:cv.AdjusterAdapter.Dispose" decl="false" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="644">
pure virtual interface

</member>
<member name="M:cv.AdjusterAdapter.tooFew(System.Int32,System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="647">
too few features were detected so, adjust the detector params accordingly
     * \param min the minimum number of desired features
     * \param n_detected the number previously detected

</member>
<member name="M:cv.AdjusterAdapter.tooMany(System.Int32,System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="652">
too many features were detected so, adjust the detector params accordingly
     * \param max the maximum number of desired features
     * \param n_detected the number previously detected

</member>
<member name="M:cv.AdjusterAdapter.good" decl="false" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="657">
are params maxed out or still valid?
     * \return false if the parameters can't be adjusted any more

</member>
<!-- Discarding badly formed XML document comment for member 'T:cv.DynamicAdaptedFeatureDetector'. -->
<member name="M:cv.DynamicAdaptedFeatureDetector.#ctor(cv.Ptr&lt;cv.AdjusterAdapter&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,System.Int32,System.Int32,System.Int32)" decl="true" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="682">
\param adjuster an AdjusterAdapter that will do the detection and parameter adjustment
     *  \param max_features the maximum desired number of features
     *  \param max_iters the maximum number of times to try to adjust the feature detector params
     *          for the FastAdjuster this can be high, but with Star or Surf this can get time consuming
     *  \param min_features the minimum desired features

</member>
<member name="T:cv.FastAdjuster" decl="false" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="704">
\brief an adjust for the FAST detector. This will basically decrement or increment the
 * threshold by 1

</member>
<member name="M:cv.FastAdjuster.#ctor(System.Int32,System.Boolean,System.Int32,System.Int32)" decl="true" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="710">
\param init_thresh the initial threshold to start with, default = 20
     * \param nonmax whether to use non max or not for fast feature detection

</member>
<member name="T:cv.StarAdjuster" decl="false" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="730">
An adjuster for StarFeatureDetector, this one adjusts the responseThreshold for now
 * TODO find a faster way to converge the parameters for Star - use CvStarDetectorParams

</member>
<member name="M:cv.BriefDescriptorExtractor.info" decl="true" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="818">
@todo read and write for brief
</member>
<member name="M:cv.Hamming.op_FunctionCall(System.Byte!System.Runtime.CompilerServices.IsConst*,System.Byte!System.Runtime.CompilerServices.IsConst*,System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\features2d\features2d.hpp" line="905">
this will count the bits in a ^ b

</member>
<member name="M:cvflann.any.#ctor" decl="false" source="c:\opencv-242\build\include\opencv2\flann\any.h" line="178">
Empty constructor.
</member>
<member name="M:cvflann.any.#ctor(System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte!System.Runtime.CompilerServices.IsConst*)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\any.h" line="183">
Special initializing constructor for string literals.
</member>
<member name="M:cvflann.any.#ctor(cvflann.any!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\any.h" line="190">
Copy constructor.
</member>
<member name="M:cvflann.any.Dispose" decl="false" source="c:\opencv-242\build\include\opencv2\flann\any.h" line="197">
Destructor.
</member>
<member name="M:cvflann.any.assign(cvflann.any!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\any.h" line="203">
Assignment function from another any.
</member>
<member name="M:cvflann.any.op_Assign(System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte!System.Runtime.CompilerServices.IsConst*)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\any.h" line="229">
Assignment operator, specialed for literal strings.
They have types like const char [6] which don't work as expected.
</member>
<member name="M:cvflann.any.swap(cvflann.any*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\any.h" line="236">
Utility functions
</member>
<member name="M:cvflann.any.empty" decl="false" source="c:\opencv-242\build\include\opencv2\flann\any.h" line="263">
Returns true if the any contains no value.
</member>
<member name="M:cvflann.any.reset" decl="false" source="c:\opencv-242\build\include\opencv2\flann\any.h" line="269">
Frees any allocated memory, and sets the value to NULL.
</member>
<member name="M:cvflann.any.compatible(cvflann.any!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\any.h" line="276">
Returns true if the two types are the same.
</member>
<member name="T:cvflann.IndexHeader" decl="false" source="c:\opencv-242\build\include\opencv2\flann\saving.h" line="66">
Structure representing the index header.

</member>
<member name="M:cvflann.load_header(_iobuf*)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\saving.h" line="102">

 @param stream - Stream to load from
 @return Index header

</member>
<member name="T:cvflann.HammingLUT" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dist.h" line="369">
Hamming distance functor - counts the bit differences between two strings - useful for the Brief descriptor
bit count of A exclusive XOR'ed with B

</member>
<member name="M:cvflann.HammingLUT.op_FunctionCall(System.Byte!System.Runtime.CompilerServices.IsConst*,System.Byte!System.Runtime.CompilerServices.IsConst*,System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dist.h" line="381">
this will count the bits in a ^ b

</member>
<member name="T:cvflann.DynamicBitset" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="53">
Class re-implementing the boost version of it
 * This helps not depending on boost, it also does not do the bound checks
 * and has a way to reset a block for speed

</member>
<member name="M:cvflann.DynamicBitset.#ctor" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="60">
@param default constructor

</member>
<member name="M:cvflann.DynamicBitset.#ctor(System.UInt32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="66">
@param only constructor we use in our code
     * @param the size of the bitset (in bits)

</member>
<member name="M:cvflann.DynamicBitset.clear" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="75">
Sets all the bits to 0

</member>
<member name="M:cvflann.DynamicBitset.empty" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="82">
@brief checks if the bitset is empty
     * @return true if the bitset is empty

</member>
<member name="M:cvflann.DynamicBitset.reset" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="90">
@param set all the bits to 0

</member>
<member name="M:cvflann.DynamicBitset.reset(System.UInt32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="97">
@brief set one bit to 0
     * @param

</member>
<member name="M:cvflann.DynamicBitset.reset_block(System.UInt32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="105">
@brief sets a specific bit to 0, and more bits too
     * This function is useful when resetting a given set of bits so that the
     * whole bitset ends up being 0: if that's the case, we don't care about setting
     * other bits to 0
     * @param

</member>
<member name="M:cvflann.DynamicBitset.resize(System.UInt32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="116">
@param resize the bitset so that it contains at least size bits
     * @param size

</member>
<member name="M:cvflann.DynamicBitset.set(System.UInt32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="125">
@param set a bit to true
     * @param index the index of the bit to set to 1

</member>
<member name="M:cvflann.DynamicBitset.size" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="133">
@param gives the number of contained bits

</member>
<member name="M:cvflann.DynamicBitset.test(System.UInt32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\dynamic_bitset.h" line="140">
@param check if a bit is set
     * @param index the index of the bit to check
     * @return true if the bit is set

</member>
<member name="M:cvflann.PooledAllocator.#ctor(System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\allocator.h" line="92">
Default constructor. Initializes a new pool.

</member>
<member name="M:cvflann.PooledAllocator.Dispose" decl="false" source="c:\opencv-242\build\include\opencv2\flann\allocator.h" line="105">
Destructor. Frees all the memory allocated in this pool.

</member>
<member name="M:cvflann.PooledAllocator.allocateMemory(System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\allocator.h" line="119">
Returns a pointer to a piece of new memory of the given size in bytes
allocated from the pool.

</member>
<member name="M:cvflann.seed_random(System.UInt32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\random.h" line="43">
Seeds the random number generator
 @param seed Random seed

</member>
<member name="M:cvflann.rand_double(System.Double,System.Double)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\random.h" line="55">
Generates a random double value.
@param high Upper limit
@param low Lower limit
@return Random double value

</member>
<member name="M:cvflann.rand_int(System.Int32,System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\random.h" line="66">
Generates a random integer value.
@param high Upper limit
@param low Lower limit
@return Random integer value

</member>
<member name="T:cvflann.UniqueRandom" decl="false" source="c:\opencv-242\build\include\opencv2\flann\random.h" line="77">
Random number generator that returns a distinct number from
the [0,n) interval each time.

</member>
<member name="M:cvflann.UniqueRandom.#ctor(System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\random.h" line="88">
Constructor.
@param n Size of the interval from which to generate
@return

</member>
<member name="M:cvflann.UniqueRandom.init(System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\random.h" line="98">
Initializes the number generator.
@param n the size of the interval from which to generate random numbers.

</member>
<member name="M:cvflann.UniqueRandom.next" decl="false" source="c:\opencv-242\build\include\opencv2\flann\random.h" line="115">
Return a distinct random integer in greater or equal to 0 and less
than 'n' on each call. It should be called maximum 'n' times.
Returns: a random integer

</member>
<member name="M:cvflann.Logger.setLevel(System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\logger.h" line="81">
Sets the logging level. All messages with lower priority will be ignored.
@param level Logging level

</member>
<member name="M:cvflann.Logger.setDestination(System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte!System.Runtime.CompilerServices.IsConst*)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\logger.h" line="87">
Sets the logging destination
@param name Filename or NULL for console

</member>
<member name="M:cvflann.Logger.log(System.Int32,System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte!System.Runtime.CompilerServices.IsConst*,BTEllipsis)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\logger.h" line="93">
Print log message
@param level Log level
@param fmt Message format
@return

</member>
<member name="T:cvflann.CompositeIndexParams" decl="false" source="c:\opencv-242\build\include\opencv2\flann\composite_index.h" line="42">
Index parameters for the CompositeIndex.

</member>
<member name="D:cvflann.lsh.FeatureIndex" decl="false" source="c:\opencv-242\build\include\opencv2\flann\lsh_table.h" line="67">
What is stored in an LSH bucket

</member>
<member name="D:cvflann.lsh.BucketKey" decl="false" source="c:\opencv-242\build\include\opencv2\flann\lsh_table.h" line="70">
The id from which we can get a bucket back in an LSH table

</member>
<member name="D:cvflann.lsh.Bucket" decl="false" source="c:\opencv-242\build\include\opencv2\flann\lsh_table.h" line="74">
A bucket in an LSH table

</member>
<member name="T:cvflann.lsh.LshStats" decl="false" source="c:\opencv-242\build\include\opencv2\flann\lsh_table.h" line="80">
POD for stats about an LSH table

</member>
<member name="F:cvflann.lsh.LshStats.size_histogram_" decl="false" source="c:\opencv-242\build\include\opencv2\flann\lsh_table.h" line="91">
Each contained vector contains three value: beginning/end for interval, number of elements in the bin

</member>
<!-- Discarding badly formed XML document comment for member 'M:cvflann.lsh.op_LeftShift(std.basic_ostream<System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte,std.char_traits{System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte}>*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,cvflann.lsh.LshStats!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)'. -->
<member name="T:cvflann.StartStopTimer" decl="false" source="c:\opencv-242\build\include\opencv2\flann\timer.h" line="40">
 A start-stop timer class.

 Can be used to time portions of code.

</member>
<member name="F:cvflann.StartStopTimer.value" decl="false" source="c:\opencv-242\build\include\opencv2\flann\timer.h" line="50">
Value of the timer.

</member>
<member name="M:cvflann.StartStopTimer.#ctor" decl="false" source="c:\opencv-242\build\include\opencv2\flann\timer.h" line="56">
Constructor.

</member>
<member name="M:cvflann.StartStopTimer.start" decl="false" source="c:\opencv-242\build\include\opencv2\flann\timer.h" line="64">
Starts the timer.

</member>
<member name="M:cvflann.StartStopTimer.stop" decl="false" source="c:\opencv-242\build\include\opencv2\flann\timer.h" line="72">
Stops the timer and updates timer value.

</member>
<member name="M:cvflann.StartStopTimer.reset" decl="false" source="c:\opencv-242\build\include\opencv2\flann\timer.h" line="81">
Resets the timer value to 0.

</member>
<member name="M:cvflann.log_verbosity(System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\flann\flann_base.hpp" line="49">
Sets the log level used for all flann functions
@param level Verbosity level

</member>
<member name="T:cvflann.SavedIndexParams" decl="false" source="c:\opencv-242\build\include\opencv2\flann\flann_base.hpp" line="60">
(Deprecated) Index parameters for creating a saved index.

</member>
<member name="T:cv.linemod.Feature" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="627">
@todo Convert doxy comments to rst
\brief Discriminant feature described by its location and label.

</member>
<member name="T:cv.linemod.QuantizedPyramid" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="658">
\brief Represents a modality operating over an image pyramid.

</member>
<member name="M:cv.linemod.QuantizedPyramid.quantize(cv.Mat*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="667">
 \brief Compute quantized image at current pyramid level for online detection.

 \param[out] dst The destination 8-bit image. For each pixel at most one bit is set,
                 representing its classification.

</member>
<member name="M:cv.linemod.QuantizedPyramid.extractTemplate(cv.linemod.Template*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="675">
 \brief Extract most discriminant features at current pyramid level to form a new template.

 \param[out] templ The new template.

</member>
<member name="M:cv.linemod.QuantizedPyramid.pyrDown" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="682">
 \brief Go to the next pyramid level.

 \todo Allow pyramid scale factor other than 2

</member>
<member name="T:cv.linemod.QuantizedPyramid.Candidate" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="690">
Candidate feature with a score
</member>
<member name="M:cv.linemod.QuantizedPyramid.Candidate.op_LessThan(cv.linemod.QuantizedPyramid.Candidate!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="695">
Sort candidates with high score to the front
</member>
<member name="M:cv.linemod.QuantizedPyramid.selectScatteredFeatures(std.vector&lt;cv.linemod.QuantizedPyramid.Candidate&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,std.vector&lt;cv.linemod.Feature&gt;*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,System.UInt32,System.Single)" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="705">
 \brief Choose candidate features so that they are not bunched together.

 \param[in]  candidates   Candidate features sorted by score.
 \param[out] features     Destination vector of selected features.
 \param[in]  num_features Number of candidates to select.
 \param[in]  distance     Hint for desired distance between features.

</member>
<member name="T:cv.linemod.Modality" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="720">
 \brief Interface for modalities that plug into the LINE template matching representation.

 \todo Max response, to allow optimization of summing (255/MAX) features as uint8

</member>
<member name="M:cv.linemod.Modality.process(cv.Mat!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,cv.Mat!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="731">
 \brief Form a quantized image pyramid from a source image.

 \param[in] src  The source image. Type depends on the modality.
 \param[in] mask Optional mask. If not empty, unmasked pixels are set to zero
                 in quantized image and cannot be extracted as features.

</member>
<member name="M:cv.linemod.Modality.create(std.basic_string&lt;System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte,std.char_traits{System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte},std.allocator&lt;System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte&gt;&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="749">
 \brief Create modality by name.

 The following modality types are supported:
 - "ColorGradient"
 - "DepthNormal"

</member>
<member name="M:cv.linemod.Modality.create(cv.FileNode!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="758">
\brief Load a modality from file.

</member>
<member name="T:cv.linemod.ColorGradient" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="769">
\brief Modality that computes quantized gradient orientations from a color image.

</member>
<member name="M:cv.linemod.ColorGradient.#ctor" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="775">
\brief Default constructor. Uses reasonable default parameter values.

</member>
<member name="M:cv.linemod.ColorGradient.#ctor(System.Single,System.UInt32,System.Single)" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="780">
 \brief Constructor.

 \param weak_threshold   When quantizing, discard gradients with magnitude less than this.
 \param num_features     How many features a template must contain.
 \param strong_threshold Consider as candidate features only gradients whose norms are
                         larger than this.

</member>
<member name="T:cv.linemod.DepthNormal" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="804">
\brief Modality that computes quantized surface normals from a dense depth map.

</member>
<member name="M:cv.linemod.DepthNormal.#ctor" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="810">
\brief Default constructor. Uses reasonable default parameter values.

</member>
<member name="M:cv.linemod.DepthNormal.#ctor(System.Int32,System.Int32,System.UInt32,System.Int32)" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="815">
 \brief Constructor.

 \param distance_threshold   Ignore pixels beyond this distance.
 \param difference_threshold When computing normals, ignore contributions of pixels whose
                             depth difference with the central pixel is above this threshold.
 \param num_features         How many features a template must contain.
 \param extract_threshold    Consider as candidate feature only if there are no differing
                             orientations within a distance of extract_threshold.

</member>
<member name="M:cv.linemod.colormap(cv.Mat!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,cv.Mat*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="843">
\brief Debug function to colormap a quantized image for viewing.

</member>
<member name="T:cv.linemod.Match" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="848">
\brief Represents a successful template match.

</member>
<member name="M:cv.linemod.Match.op_LessThan(cv.linemod.Match!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="859">
Sort matches with high similarity to the front
</member>
<member name="T:cv.linemod.Detector" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="886">
\brief Object detector using the LINE template matching algorithm with any set of
modalities.

</member>
<member name="M:cv.linemod.Detector.#ctor" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="893">
\brief Empty constructor, initialize with read().

</member>
<member name="M:cv.linemod.Detector.#ctor(std.vector&lt;cv.Ptr&lt;cv.linemod.Modality&gt;&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,std.vector&lt;System.Int32&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="898">
 \brief Constructor.

 \param modalities       Modalities to use (color gradients, depth normals, ...).
 \param T_pyramid        Value of the sampling step T at each pyramid level. The
                         number of pyramid levels is T_pyramid.size().

</member>
<!-- Discarding badly formed XML document comment for member 'M:cv.linemod.Detector.match(std.vector<cv.Mat>!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,System.Single,std.vector<cv.linemod.Match>*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,std.vector<std.basic_string<System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte,std.char_traits{System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte},std.allocator<System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte>>>!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,cv._OutputArray!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,std.vector<cv.Mat>!System.Runtime.CompilerServices.IsCon'. -->
<member name="M:cv.linemod.Detector.addTemplate(std.vector&lt;cv.Mat&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,std.basic_string&lt;System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte,std.char_traits{System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte},std.allocator&lt;System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte&gt;&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,cv.Mat!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,cv.Rect_&lt;System.Int32&gt;*)" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="927">
 \brief Add new object template.

 \param      sources      Source images, one for each modality.
 \param      class_id     Object class ID.
 \param      object_mask  Mask separating object from background.
 \param[out] bounding_box Optionally return bounding box of the extracted features.

 \return Template ID, or -1 if failed to extract a valid template.

</member>
<member name="M:cv.linemod.Detector.addSyntheticTemplate(std.vector&lt;cv.linemod.Template&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,std.basic_string&lt;System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte,std.char_traits{System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte},std.allocator&lt;System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte&gt;&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced)" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="940">
\brief Add a new object template computed by external means.

</member>
<member name="M:cv.linemod.Detector.getModalities" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="945">
 \brief Get the modalities used by this detector.

 You are not permitted to add/remove modalities, but you may dynamic_cast them to
 tweak parameters.

</member>
<member name="M:cv.linemod.Detector.getT(System.Int32)" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="953">
\brief Get sampling step T at pyramid_level.

</member>
<member name="M:cv.linemod.Detector.pyramidLevels" decl="false" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="958">
\brief Get number of pyramid levels used by this detector.

</member>
<member name="M:cv.linemod.Detector.getTemplates(std.basic_string&lt;System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte,std.char_traits{System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte},std.allocator&lt;System.SByte!System.Runtime.CompilerServices.IsSignUnspecifiedByte&gt;&gt;!System.Runtime.CompilerServices.IsConst*!System.Runtime.CompilerServices.IsImplicitlyDereferenced,System.Int32)" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="963">
 \brief Get the template pyramid identified by template_id.

 For example, with 2 modalities (Gradient, Normal) and two pyramid levels
 (L0, L1), the order is (GradientL0, NormalL0, GradientL1, NormalL1).

</member>
<member name="M:cv.linemod.getDefaultLINE" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="1007">
 \brief Factory function for detector using LINE algorithm with color gradients.

 Default parameter settings suitable for VGA images.

</member>
<member name="M:cv.linemod.getDefaultLINEMOD" decl="true" source="c:\opencv-242\build\include\opencv2\objdetect\objdetect.hpp" line="1014">
 \brief Factory function for detector using LINE-MOD algorithm with color gradients
 and depth normals.

 Default parameter settings suitable for VGA images.

</member>
<member name="M:Alexa.Core.SetSourceImage(System.Drawing.Bitmap)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="63">
<summary>
Set the input image of the Alexa Core
</summary>
<param name="inputImage">the input image</param>
</member>
<member name="M:Alexa.Core.EnableDebug(System.Boolean)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="69">
<summary>
Enable/disable the debug level
</summary>
<param name="enable">set true to enable debug</param>
</member>
<member name="M:Alexa.Core.SetDebugFolder(System.String)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="75">
<summary>
Set the debug folder to save debug image
</summary>
<param name="path">the path of debug folder</param>
</member>
<member name="M:Alexa.Core.GetSourceImage" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="81">
<summary>
Get the Alexa Core source image
</summary>
<returns> Returns the source image</returns>
</member>
<member name="M:Alexa.Core.SetBrightnessContrast(System.Int32,System.Int32)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="87">
<summary>
Change brightness and contrast of the Alexa Core source image
</summary>
<param name="brightness">brightness value</param>
<param name="contrast">contrast value</param>
</member>
<member name="M:Alexa.Core.GetGenericBoxes(System.Int32,System.Int32,System.Int32)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="94">
<summary>
Find all the boxes (in the Alexa Core source image) that match the arguments.
</summary>
<param name="height">height of input box (pixel)</param>
<param name="width">width of input box (pixel)</param>
<param name="tollerance">tollerance of height and width (pixel)</param>
<returns> Returns all input boxes found</returns>
</member>
<member name="M:Alexa.Core.GetGenericBoxesV2(System.Int32,System.Int32,System.Int32)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="103">
<summary>
Find all the boxes (in the Alexa Core source image) that match the arguments.
</summary>
<param name="height">height of input box (pixel)</param>
<param name="width">width of input box (pixel)</param>
<param name="tollerance">tollerance of height and width (pixel)</param>
<returns> Returns all input boxes found</returns>
</member>
<member name="M:Alexa.Core.GetInputBoxes" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="112">
<summary>
Find all the input boxes in the Alexa Core source image.
</summary>
<returns> Returns all input boxes found</returns>
</member>
<member name="M:Alexa.Core.GetInputBoxesV2" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="118">
<summary>
Find all the input boxes in the Alexa Core source image.
</summary>
<returns> Returns all input boxes found</returns>
</member>
<member name="M:Alexa.Core.GetButtons" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="124">
<summary>
Find all buttons in the Alexa Core source image.
</summary>
<returns> Returns buttons found</returns>
</member>
<member name="M:Alexa.Core.GetButtonsV2" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="130">
<summary>
Find all buttons in the Alexa Core source image.
</summary>
<returns> Returns buttons found</returns>
</member>
<member name="M:Alexa.Core.GetIconListBoxes" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="136">
<summary>
Find all list boxes in the Alexa Core source image.
</summary>
<returns> Returns all List Boxes found</returns>
</member>
<member name="M:Alexa.Core.GetIconListBoxesV2" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="142">
<summary>
Find all list boxes in the Alexa Core source image.
</summary>
<returns> Returns all List Boxes found</returns>
</member>
<member name="M:Alexa.Core.GetChars(System.Int32,System.Int32,System.Int32,System.Int32,System.Int32)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="148">
<summary>
Find all chars in the Alexa Core source image.
</summary>
<param name="lineHeight">line height</param>
<param name="spaceThickness">space thickness</param>
<returns> Returns all chars found</returns>
</member>
<member name="M:Alexa.Core.GetCharsV2(System.Int32,System.Int32,System.Int32,System.Int32,System.Int32)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="156">
<summary>
Find all chars in the Alexa Core source image.
</summary>
<param name="lineHeight">line height</param>
<param name="spaceThickness">space thickness</param>
<returns> Returns all chars found</returns>
</member>
<member name="M:Alexa.Core.GetWords(System.Int32,System.Int32,System.Int32,System.Int32)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="164">
<summary>
Find all chars in the Alexa Core source image.
</summary>
<param name="minHeight">min word height</param>
<param name="maxHeight">max word height</param>
<param name="minWidth">min word width</param>
<param name="maxWidth">max word width</param>
<returns> Returns all words found</returns>
</member>
<member name="M:Alexa.Core.GetWordsV2(System.Int32,System.Int32,System.Int32,System.Int32)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="174">
<summary>
Find all chars in the Alexa Core source image.
</summary>
<param name="minHeight">min word height</param>
<param name="maxHeight">max word height</param>
<param name="minWidth">min word width</param>
<param name="maxWidth">max word width</param>
<returns> Returns all words found</returns>
</member>
<member name="M:Alexa.Core.GetInterestPoints" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="184">
<summary>
Find all boxes in the Alexa Core source image.
</summary>
<returns> Returns all boxes</returns>
</member>
<member name="M:Alexa.Core.FindIcon(System.Drawing.Bitmap,System.Double)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="190">
<summary>
Find the icon in the Alexa Core source image.
</summary>
<param name="icon">the icon to find</param>
<param name="threshold">the threshold</param>
<returns>Returns the coordinates of the icon</returns>
</member>
<member name="M:Alexa.Core.BinarizeImage" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="198">
<summary>
Binarize the Alexa Core source image
</summary>
</member>
<member name="M:Alexa.Core.ReplaceColor(System.Drawing.Bitmap,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="203">
<summary>
Replace color into the Alexa Core source image
</summary>
<param name="oldR">old red component</param>
<param name="oldG">old green component</param>
<param name="oldB">old blue component</param>
<param name="newR">new red component</param>
<param name="newG">new green component</param>
<param name="newB">new blue component</param>
</member>
<member name="M:Alexa.Core.Release" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="214">
<summary>
Release all objects of Alexa Core
</summary>
</member>
<member name="M:Alexa.Core.GetBoxes(System.Int32)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="244">
<summary>
Find boxes in the Alexa Core source image.
</summary>
<returns> Returns all boxes</returns>
</member>
<member name="M:Alexa.Core.GetBoxes2(System.Int32)" decl="true" source="c:\work\githubrepos\alexa\alexa\al'exa.core\al'exa.core\al'exa.core.h" line="250">
<summary>
Find boxes in the Alexa Core source image.
</summary>
<returns> Returns all boxes</returns>
</member>
</members>
</doc>