#region

using System;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.Threading;
#if THREAD_SAFE
using ThreadSafe = dnlib.Threading.Collections;
#else
using ThreadSafe = System.Collections.Generic;

#endif

#endregion

namespace dnlib.DotNet
{
    /// <summary>
    ///     A high-level representation of a row in the Property table
    /// </summary>
    public abstract class PropertyDef : IHasConstant, IHasCustomAttribute, IHasSemantic, IFullName, IMemberDef
    {
        /// <summary>
        ///     The row id in its table
        /// </summary>
        protected uint rid;

#if THREAD_SAFE
		readonly Lock theLock = Lock.Create();
#endif

        /// <inheritdoc />
        public MDToken MDToken => new MDToken(Table.Property, rid);

        /// <inheritdoc />
        public uint Rid
        {
            get { return rid; }
            set { rid = value; }
        }

        /// <inheritdoc />
        public int HasConstantTag => 2;

        /// <inheritdoc />
        public int HasCustomAttributeTag => 9;

        /// <inheritdoc />
        public int HasSemanticTag => 1;

        /// <summary>
        ///     From column Property.PropFlags
        /// </summary>
        public PropertyAttributes Attributes
        {
            get { return (PropertyAttributes) attributes; }
            set { attributes = (int) value; }
        }

        /// <summary>Attributes</summary>
        protected int attributes;

        /// <summary>
        ///     From column Property.Name
        /// </summary>
        public UTF8String Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>Name</summary>
        protected UTF8String name;

        /// <summary>
        ///     From column Property.Type
        /// </summary>
        public CallingConventionSig Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary />
        protected CallingConventionSig type;

        /// <inheritdoc />
        public Constant Constant
        {
            get
            {
                if(!constant_isInitialized)
                    InitializeConstant();
                return constant;
            }
            set
            {
#if THREAD_SAFE
				theLock.EnterWriteLock(); try {
#endif
                constant = value;
                constant_isInitialized = true;
#if THREAD_SAFE
				} finally { theLock.ExitWriteLock(); }
#endif
            }
        }

        /// <summary />
        protected Constant constant;

        /// <summary />
        protected bool constant_isInitialized;

        private void InitializeConstant()
        {
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
            if(constant_isInitialized)
                return;
            constant = GetConstant_NoLock();
            constant_isInitialized = true;
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
        }

        /// <summary>Called to initialize <see cref="constant" /></summary>
        protected virtual Constant GetConstant_NoLock()
        {
            return null;
        }

        /// <summary>Reset <see cref="Constant" /></summary>
        protected void ResetConstant()
        {
            constant_isInitialized = false;
        }

        /// <summary>
        ///     Gets all custom attributes
        /// </summary>
        public CustomAttributeCollection CustomAttributes
        {
            get
            {
                if(customAttributes == null)
                    InitializeCustomAttributes();
                return customAttributes;
            }
        }

        /// <summary />
        protected CustomAttributeCollection customAttributes;

        /// <summary>Initializes <see cref="customAttributes" /></summary>
        protected virtual void InitializeCustomAttributes()
        {
            Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
        }

        /// <summary>
        ///     Gets/sets the first getter method. Writing <c>null</c> will clear all get methods.
        /// </summary>
        public MethodDef GetMethod
        {
            get
            {
                if(otherMethods == null)
                    InitializePropertyMethods();
                return getMethods.Get(0, null);
            }
            set
            {
                if(otherMethods == null)
                    InitializePropertyMethods();
                if(value == null)
                    getMethods.Clear();
                else if(getMethods.Count == 0)
                    getMethods.Add(value);
                else
                    getMethods.Set(0, value);
            }
        }

        /// <summary>
        ///     Gets/sets the first setter method. Writing <c>null</c> will clear all set methods.
        /// </summary>
        public MethodDef SetMethod
        {
            get
            {
                if(otherMethods == null)
                    InitializePropertyMethods();
                return setMethods.Get(0, null);
            }
            set
            {
                if(otherMethods == null)
                    InitializePropertyMethods();
                if(value == null)
                    setMethods.Clear();
                else if(setMethods.Count == 0)
                    setMethods.Add(value);
                else
                    setMethods.Set(0, value);
            }
        }

        /// <summary>
        ///     Gets all getter methods
        /// </summary>
        public ThreadSafe.IList<MethodDef> GetMethods
        {
            get
            {
                if(otherMethods == null)
                    InitializePropertyMethods();
                return getMethods;
            }
        }

        /// <summary>
        ///     Gets all setter methods
        /// </summary>
        public ThreadSafe.IList<MethodDef> SetMethods
        {
            get
            {
                if(otherMethods == null)
                    InitializePropertyMethods();
                return setMethods;
            }
        }

        /// <summary>
        ///     Gets the other methods
        /// </summary>
        public ThreadSafe.IList<MethodDef> OtherMethods
        {
            get
            {
                if(otherMethods == null)
                    InitializePropertyMethods();
                return otherMethods;
            }
        }

        private void InitializePropertyMethods()
        {
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
            if(otherMethods == null)
                InitializePropertyMethods_NoLock();
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
        }

        /// <summary>
        ///     Initializes <see cref="otherMethods" />, <see cref="getMethods" />,
        ///     and <see cref="setMethods" />.
        /// </summary>
        protected virtual void InitializePropertyMethods_NoLock()
        {
            getMethods = ThreadSafeListCreator.Create<MethodDef>();
            setMethods = ThreadSafeListCreator.Create<MethodDef>();
            otherMethods = ThreadSafeListCreator.Create<MethodDef>();
        }

        /// <summary />
        protected ThreadSafe.IList<MethodDef> getMethods;

        /// <summary />
        protected ThreadSafe.IList<MethodDef> setMethods;

        /// <summary />
        protected ThreadSafe.IList<MethodDef> otherMethods;

        /// <summary>Reset <see cref="GetMethods" />, <see cref="SetMethods" />, <see cref="OtherMethods" /></summary>
        protected void ResetMethods()
        {
            otherMethods = null;
        }

        /// <summary>
        ///     <c>true</c> if there are no methods attached to this property
        /// </summary>
        public bool IsEmpty => GetMethods.Count == 0 &&
                               setMethods.Count == 0 &&
                               otherMethods.Count == 0;

        /// <inheritdoc />
        public bool HasCustomAttributes => CustomAttributes.Count > 0;

        /// <summary>
        ///     <c>true</c> if <see cref="OtherMethods" /> is not empty
        /// </summary>
        public bool HasOtherMethods => OtherMethods.Count > 0;

        /// <summary>
        ///     <c>true</c> if <see cref="Constant" /> is not <c>null</c>
        /// </summary>
        public bool HasConstant => Constant != null;

        /// <summary>
        ///     Gets the constant element type or <see cref="dnlib.DotNet.ElementType.End" /> if there's no constant
        /// </summary>
        public ElementType ElementType
        {
            get
            {
                var c = Constant;
                return c == null ? ElementType.End : c.Type;
            }
        }

        /// <summary>
        ///     Gets/sets the property sig
        /// </summary>
        public PropertySig PropertySig
        {
            get { return type as PropertySig; }
            set { type = value; }
        }

        /// <summary>
        ///     Gets/sets the declaring type (owner type)
        /// </summary>
        public TypeDef DeclaringType
        {
            get { return declaringType2; }
            set
            {
                var currentDeclaringType = DeclaringType2;
                if(currentDeclaringType == value)
                    return;
                if(currentDeclaringType != null)
                    currentDeclaringType.Properties.Remove(this); // Will set DeclaringType2 = null
                if(value != null)
                    value.Properties.Add(this); // Will set DeclaringType2 = value
            }
        }

        /// <inheritdoc />
        ITypeDefOrRef IMemberRef.DeclaringType => declaringType2;

        /// <summary>
        ///     Called by <see cref="DeclaringType" /> and should normally not be called by any user
        ///     code. Use <see cref="DeclaringType" /> instead. Only call this if you must set the
        ///     declaring type without inserting it in the declaring type's method list.
        /// </summary>
        public TypeDef DeclaringType2
        {
            get { return declaringType2; }
            set { declaringType2 = value; }
        }

        /// <summary />
        protected TypeDef declaringType2;

        /// <inheritdoc />
        public ModuleDef Module
        {
            get
            {
                var dt = declaringType2;
                return dt == null ? null : dt.Module;
            }
        }

        /// <summary>
        ///     Gets the full name of the property
        /// </summary>
        public string FullName
        {
            get
            {
                var dt = declaringType2;
                return FullNameCreator.PropertyFullName(dt == null ? null : dt.FullName, name, type);
            }
        }

        bool IIsTypeOrMethod.IsType => false;

        bool IIsTypeOrMethod.IsMethod => false;

        bool IMemberRef.IsField => false;

        bool IMemberRef.IsTypeSpec => false;

        bool IMemberRef.IsTypeRef => false;

        bool IMemberRef.IsTypeDef => false;

        bool IMemberRef.IsMethodSpec => false;

        bool IMemberRef.IsMethodDef => false;

        bool IMemberRef.IsMemberRef => false;

        bool IMemberRef.IsFieldDef => false;

        bool IMemberRef.IsPropertyDef => true;

        bool IMemberRef.IsEventDef => false;

        bool IMemberRef.IsGenericParam => false;

        /// <summary>
        ///     Set or clear flags in <see cref="attributes" />
        /// </summary>
        /// <param name="set">
        ///     <c>true</c> if flags should be set, <c>false</c> if flags should
        ///     be cleared
        /// </param>
        /// <param name="flags">Flags to set or clear</param>
        private void ModifyAttributes(bool set, PropertyAttributes flags)
        {
#if THREAD_SAFE
			int origVal, newVal;
			do {
				origVal = attributes;
				if (set)
					newVal = origVal | (int)flags;
				else
					newVal = origVal & ~(int)flags;
			} while (Interlocked.CompareExchange(ref attributes, newVal, origVal) != origVal);
#else
            if(set)
                attributes |= (int) flags;
            else
                attributes &= ~(int) flags;
#endif
        }

        /// <summary>
        ///     Gets/sets the <see cref="PropertyAttributes.SpecialName" /> bit
        /// </summary>
        public bool IsSpecialName
        {
            get { return ((PropertyAttributes) attributes & PropertyAttributes.SpecialName) != 0; }
            set { ModifyAttributes(value, PropertyAttributes.SpecialName); }
        }

        /// <summary>
        ///     Gets/sets the <see cref="PropertyAttributes.RTSpecialName" /> bit
        /// </summary>
        public bool IsRuntimeSpecialName
        {
            get { return ((PropertyAttributes) attributes & PropertyAttributes.RTSpecialName) != 0; }
            set { ModifyAttributes(value, PropertyAttributes.RTSpecialName); }
        }

        /// <summary>
        ///     Gets/sets the <see cref="PropertyAttributes.HasDefault" /> bit
        /// </summary>
        public bool HasDefault
        {
            get { return ((PropertyAttributes) attributes & PropertyAttributes.HasDefault) != 0; }
            set { ModifyAttributes(value, PropertyAttributes.HasDefault); }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }
    }

    /// <summary>
    ///     A Property row created by the user and not present in the original .NET file
    /// </summary>
    public class PropertyDefUser : PropertyDef
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public PropertyDefUser()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public PropertyDefUser(UTF8String name)
            : this(name, null)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="sig">Property signature</param>
        public PropertyDefUser(UTF8String name, PropertySig sig)
            : this(name, sig, 0)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="sig">Property signature</param>
        /// <param name="flags">Flags</param>
        public PropertyDefUser(UTF8String name, PropertySig sig, PropertyAttributes flags)
        {
            this.name = name;
            type = sig;
            attributes = (int) flags;
        }
    }

    /// <summary>
    ///     Created from a row in the Property table
    /// </summary>
    internal sealed class PropertyDefMD : PropertyDef, IMDTokenProviderMD
    {
        /// <summary>The module where this instance is located</summary>
        private readonly ModuleDefMD readerModule;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="readerModule">The module which contains this <c>Property</c> row</param>
        /// <param name="rid">Row ID</param>
        /// <exception cref="ArgumentNullException">If <paramref name="readerModule" /> is <c>null</c></exception>
        /// <exception cref="ArgumentException">If <paramref name="rid" /> is invalid</exception>
        public PropertyDefMD(ModuleDefMD readerModule, uint rid)
        {
#if DEBUG
            if(readerModule == null)
                throw new ArgumentNullException("readerModule");
            if(readerModule.TablesStream.PropertyTable.IsInvalidRID(rid))
                throw new BadImageFormatException(string.Format("Property rid {0} does not exist", rid));
#endif
            OrigRid = rid;
            this.rid = rid;
            this.readerModule = readerModule;
            uint name;
            var type = readerModule.TablesStream.ReadPropertyRow(OrigRid, out attributes, out name);
            this.name = readerModule.StringsStream.ReadNoNull(name);
            declaringType2 = readerModule.GetOwnerType(this);
            this.type = readerModule.ReadSignature(type, new GenericParamContext(declaringType2));
        }

        /// <inheritdoc />
        public uint OrigRid
        {
            get;
        }

        /// <inheritdoc />
        protected override Constant GetConstant_NoLock()
        {
            return readerModule.ResolveConstant(readerModule.MetaData.GetConstantRid(Table.Property, OrigRid));
        }

        /// <inheritdoc />
        protected override void InitializeCustomAttributes()
        {
            var list = readerModule.MetaData.GetCustomAttributeRidList(Table.Property, OrigRid);
            var tmp = new CustomAttributeCollection((int) list.Length, list, (list2, index) => readerModule.ReadCustomAttribute(((RidList) list2)[index]));
            Interlocked.CompareExchange(ref customAttributes, tmp, null);
        }

        internal PropertyDefMD InitializeAll()
        {
            MemberMDInitializer.Initialize(Attributes);
            MemberMDInitializer.Initialize(Name);
            MemberMDInitializer.Initialize(Type);
            MemberMDInitializer.Initialize(Constant);
            MemberMDInitializer.Initialize(CustomAttributes);
            MemberMDInitializer.Initialize(GetMethod);
            MemberMDInitializer.Initialize(SetMethod);
            MemberMDInitializer.Initialize(OtherMethods);
            MemberMDInitializer.Initialize(DeclaringType);
            return this;
        }

        /// <inheritdoc />
        protected override void InitializePropertyMethods_NoLock()
        {
            if(otherMethods != null)
                return;
            ThreadSafe.IList<MethodDef> newOtherMethods;
            ThreadSafe.IList<MethodDef> newGetMethods, newSetMethods;
            var dt = declaringType2 as TypeDefMD;
            if(dt == null)
            {
                newGetMethods = ThreadSafeListCreator.Create<MethodDef>();
                newSetMethods = ThreadSafeListCreator.Create<MethodDef>();
                newOtherMethods = ThreadSafeListCreator.Create<MethodDef>();
            }
            else
            {
                dt.InitializeProperty(this, out newGetMethods, out newSetMethods, out newOtherMethods);
            }
            getMethods = newGetMethods;
            setMethods = newSetMethods;
            // Must be initialized last
            otherMethods = newOtherMethods;
        }
    }
}