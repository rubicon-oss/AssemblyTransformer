// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using Mono.Cecil;

namespace MethodVirtualizer.ILCodeGeneration
{
  public interface ICodeGenerator
  {
    MethodDefinition GenerateMethodAndMoveBody (MethodDefinition originalMethod, string introducedMethodName);
  }
}