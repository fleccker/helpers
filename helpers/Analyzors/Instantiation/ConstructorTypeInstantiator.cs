using System;

namespace helpers.Analyzors.Instantiation
{
    public class ConstructorTypeInstantiator : ITypeInstantiator
    {
        public byte Speed => 100;

        public object Instantiate(TypeAnalyzorResult type, object[] args)
        {
            foreach (var constructor in type.Constructors)
            {
                var constructorArgs = constructor.GetParameters();

                if (constructorArgs != null && constructorArgs.Length > 0)
                {
                    if (args != null)
                    {
                        if (args.Length != constructorArgs.Length)
                            continue;

                        bool canContinue = true;

                        for (int i = 0; i < constructorArgs.Length; i++)
                        {
                            var argType = constructorArgs[i].ParameterType;

                            if (args[i] is null)
                                continue;

                            if (args[i].GetType() != argType)
                            {
                                canContinue = false;
                                break;
                            }
                        }

                        if (!canContinue)
                            continue;

                        try
                        {
                            return constructor.Invoke(args);
                        }
                        catch { continue; }
                    }
                }
                else
                {
                    return constructor.Invoke(null);
                }
            }

            return null;
        }

        public bool IsAvailable(Type type) => true;
    }
}